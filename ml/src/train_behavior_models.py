from __future__ import annotations

import json
import pickle
from pathlib import Path

import pandas as pd
from imblearn.over_sampling import SMOTE
from sklearn.ensemble import RandomForestClassifier
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score, classification_report, confusion_matrix, f1_score
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.tree import DecisionTreeClassifier

PROCESSED_DATA_PATH = Path(__file__).resolve().parent.parent / "data" / "processed" / "behavior_features.csv"
MODEL_DIR = Path(__file__).resolve().parent.parent / "models"
METRICS_PATH = MODEL_DIR / "behavior_metrics.json"
COMPARISON_PATH = MODEL_DIR / "behavior_comparison_results.json"

BASE_FEATURE_COLUMNS = ["sleep", "steps", "diet", "social", "hobbies", "stress"]
ENGINEERED_FEATURE_COLUMNS = [
    "low_sleep",
    "low_social",
    "inactive",
    "healthy_diet",
    "high_hobby_time",
    "high_stress",
]
FEATURE_COLUMNS = [*BASE_FEATURE_COLUMNS, *ENGINEERED_FEATURE_COLUMNS]
TARGET_COLUMN = "risk_level"
CLASS_ORDER = ["low", "high"]


def build_models() -> dict[str, tuple[object, bool]]:
    return {
        "logistic_regression": (
            LogisticRegression(max_iter=1000, class_weight="balanced", solver="liblinear"),
            True,
        ),
        "decision_tree": (DecisionTreeClassifier(class_weight="balanced", random_state=42), False),
        "random_forest": (
            RandomForestClassifier(
                n_estimators=400,
                max_depth=10,
                min_samples_split=5,
                class_weight="balanced",
                random_state=42,
            ),
            False,
        ),
    }


def add_engineered_features(df: pd.DataFrame) -> pd.DataFrame:
    engineered = df.copy()
    engineered["low_sleep"] = (engineered["sleep"] < 5).astype(int)
    engineered["low_social"] = (engineered["social"] < 3).astype(int)
    engineered["inactive"] = (engineered["steps"] < 3).astype(int)
    engineered["healthy_diet"] = (engineered["diet"] >= 3).astype(int)
    engineered["high_hobby_time"] = (engineered["hobbies"] >= 5).astype(int)
    engineered["high_stress"] = (engineered["stress"] >= 4).astype(int)
    return engineered


def evaluate_model(
    name: str,
    model: object,
    use_scaler: bool,
    X_train: pd.DataFrame,
    X_test: pd.DataFrame,
    y_train: pd.Series,
    y_test: pd.Series,
) -> tuple[dict, object | None]:
    smote = SMOTE(random_state=42)
    X_train_resampled, y_train_resampled = smote.fit_resample(X_train, y_train)

    scaler = None
    if use_scaler:
        scaler = StandardScaler()
        X_train_input = scaler.fit_transform(X_train_resampled)
        X_test_input = scaler.transform(X_test)
    else:
        X_train_input = X_train_resampled
        X_test_input = X_test

    model.fit(X_train_input, y_train_resampled)
    predictions = model.predict(X_test_input)

    accuracy = accuracy_score(y_test, predictions)
    weighted_f1 = f1_score(y_test, predictions, average="weighted", zero_division=0)
    matrix = confusion_matrix(y_test, predictions, labels=CLASS_ORDER).tolist()

    print(f"\n=== Behavioral Model: {name} ===")
    print(f"Accuracy: {accuracy:.4f}")
    print(f"Weighted F1-score: {weighted_f1:.4f}")
    print("Classification Report:")
    print(classification_report(y_test, predictions, labels=CLASS_ORDER, target_names=CLASS_ORDER, zero_division=0))
    print("Confusion Matrix (low/high):")
    print(matrix)

    result = {
        "model_name": name,
        "accuracy": accuracy,
        "weighted_f1": weighted_f1,
        "confusion_matrix": matrix,
        "labels": CLASS_ORDER,
    }
    return result, scaler


def save_pickle(path: Path, value: object) -> None:
    with path.open("wb") as file:
        pickle.dump(value, file)


def main() -> None:
    if not PROCESSED_DATA_PATH.exists():
        raise FileNotFoundError(f"Processed file not found: {PROCESSED_DATA_PATH}. Run build_behavior_dataset.py first.")

    df = pd.read_csv(PROCESSED_DATA_PATH)
    missing = [column for column in [*BASE_FEATURE_COLUMNS, TARGET_COLUMN] if column not in df.columns]
    if missing:
        raise ValueError(f"Processed dataset is missing required columns: {missing}")

    df = add_engineered_features(df)
    X = df[FEATURE_COLUMNS]
    y = df[TARGET_COLUMN].astype(str).str.strip().str.lower()
    y = y.where(y.isin(CLASS_ORDER))
    valid_rows = y.notna()
    X = X.loc[valid_rows]
    y = y.loc[valid_rows]

    if y.nunique() < 2:
        raise ValueError("Both risk classes (low and high) must exist in the processed dataset.")

    print("\nClass distribution:")
    print(y.value_counts(normalize=True))

    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42, stratify=y
    )

    models = build_models()
    MODEL_DIR.mkdir(parents=True, exist_ok=True)
    results: list[dict] = []

    for model_name, (model, use_scaler) in models.items():
        result, scaler = evaluate_model(model_name, model, use_scaler, X_train, X_test, y_train, y_test)
        results.append(result)
        save_pickle(MODEL_DIR / f"behavior_{model_name}.pkl", model)
        if scaler is not None:
            save_pickle(MODEL_DIR / f"behavior_{model_name}_scaler.pkl", scaler)

    best = max(results, key=lambda item: (item["weighted_f1"], item["accuracy"]))
    best_name = best["model_name"]
    best_model = pickle.load((MODEL_DIR / f"behavior_{best_name}.pkl").open("rb"))
    best_scaler_path = MODEL_DIR / f"behavior_{best_name}_scaler.pkl"
    best_scaler = pickle.load(best_scaler_path.open("rb")) if best_scaler_path.exists() else None

    save_pickle(
        MODEL_DIR / "behavior_best_model.pkl",
        {
            "model": best_model,
            "model_name": best_name,
            "feature_columns": FEATURE_COLUMNS,
            "target_column": TARGET_COLUMN,
        },
    )
    save_pickle(MODEL_DIR / "behavior_best_scaler.pkl", best_scaler)

    metrics_payload = {
        "task": "behavioral",
        "results": results,
        "best_model": best_name,
    }
    METRICS_PATH.write_text(json.dumps(metrics_payload, indent=2), encoding="utf-8")
    COMPARISON_PATH.write_text(json.dumps(metrics_payload, indent=2), encoding="utf-8")

    print("\n=== Behavioral Comparison Summary ===")
    for row in results:
        print(
            f"{row['model_name']}: accuracy={row['accuracy']:.4f}, "
            f"weighted_f1={row['weighted_f1']:.4f}"
        )
    print(f"Best behavioral model: {best_name}")
    print(f"Saved metrics to: {METRICS_PATH}")


if __name__ == "__main__":
    main()
