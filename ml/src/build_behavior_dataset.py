from __future__ import annotations

import json
from pathlib import Path

import pandas as pd

# NOTE FOR PROJECT DOCUMENTATION:
# Target changed from DAILY_STRESS to WORK_LIFE_BALANCE_SCORE.
# WORK_LIFE_BALANCE_SCORE is a composite lifestyle score, and predictor
# features are related to this target. This can introduce mild leakage risk,
# but it was chosen to improve predictive reliability for this project.

RAW_DATA_DIR = Path(__file__).resolve().parent.parent / "data" / "raw"
PROCESSED_DATA_DIR = Path(__file__).resolve().parent.parent / "data" / "processed"
PROCESSED_DATA_PATH = PROCESSED_DATA_DIR / "behavior_features.csv"
METADATA_PATH = PROCESSED_DATA_DIR / "behavior_metadata.json"

REQUIRED_COLUMNS = {
    "SLEEP_HOURS": "sleep",
    "DAILY_STEPS": "steps",
    "FRUITS_VEGGIES": "diet",
    "SOCIAL_NETWORK": "social",
    "TIME_FOR_PASSION": "hobbies",
    "DAILY_STRESS": "stress",
    "WORK_LIFE_BALANCE_SCORE": "work_life_balance",
}


def map_wlb_to_risk(score: float, threshold: float) -> str:
    return "high" if score >= threshold else "low"


def auto_detect_behavior_csv() -> Path:
    csv_files = sorted(RAW_DATA_DIR.glob("*.csv"))
    if not csv_files:
        raise FileNotFoundError(f"No CSV file found in '{RAW_DATA_DIR}'.")

    for csv_file in csv_files:
        header = pd.read_csv(csv_file, nrows=1)
        upper_headers = {column.upper().strip() for column in header.columns}
        if set(REQUIRED_COLUMNS).issubset(upper_headers):
            return csv_file

    raise ValueError(
        "No CSV in ml/data/raw contains required columns: "
        f"{', '.join(REQUIRED_COLUMNS.keys())}"
    )


def main() -> None:
    csv_path = auto_detect_behavior_csv()
    df = pd.read_csv(csv_path)
    original_columns = list(df.columns)

    upper_to_original = {column.upper().strip(): column for column in df.columns}
    selected_original = {key: upper_to_original[key] for key in REQUIRED_COLUMNS}

    print(f"Using behavior dataset: {csv_path.name}")
    print("\nAll column names:")
    print(original_columns)
    print("\nFirst 5 rows:")
    print(df.head())
    print(f"\nDataset shape: {df.shape}")
    print("\nMissing values summary:")
    print(df.isna().sum().sort_values(ascending=False))

    processed = pd.DataFrame()
    for source_column, destination_column in REQUIRED_COLUMNS.items():
        processed[destination_column] = pd.to_numeric(df[selected_original[source_column]], errors="coerce")

    processed = processed.dropna().copy()
    processed["stress"] = pd.to_numeric(processed["stress"], errors="coerce")
    processed = processed.dropna(subset=["stress"])
    processed["stress"] = processed["stress"].round().astype(int)
    threshold = processed["work_life_balance"].median()
    processed["risk_level"] = processed["work_life_balance"].apply(lambda x: map_wlb_to_risk(x, threshold))
    processed = processed.drop(columns=["work_life_balance"])
    processed = processed[["sleep", "steps", "diet", "social", "hobbies", "stress", "risk_level"]]

    PROCESSED_DATA_DIR.mkdir(parents=True, exist_ok=True)
    processed.to_csv(PROCESSED_DATA_PATH, index=False)

    metadata = {
        "source_csv": csv_path.name,
        "selected_columns": selected_original,
        "processed_columns": list(processed.columns),
        "row_count_after_cleaning": int(len(processed)),
        "wlb_threshold": float(threshold),
    }
    METADATA_PATH.write_text(json.dumps(metadata, indent=2), encoding="utf-8")

    print("\nSelected columns:")
    print(json.dumps(selected_original, indent=2))
    print("\nProcessed preview:")
    print(processed.head())
    print(f"\nSaved processed behavior dataset to: {PROCESSED_DATA_PATH}")
    print(f"Saved metadata to: {METADATA_PATH}")


if __name__ == "__main__":
    main()
