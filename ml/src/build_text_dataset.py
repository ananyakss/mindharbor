from __future__ import annotations

import json
import re
from pathlib import Path

import pandas as pd
from sklearn.feature_extraction.text import ENGLISH_STOP_WORDS


RAW_DATA_PATH = Path(__file__).resolve().parent.parent / "data" / "raw" / "go_emotions_dataset.csv"
PROCESSED_DATA_DIR = Path(__file__).resolve().parent.parent / "data" / "processed"
PROCESSED_DATA_PATH = PROCESSED_DATA_DIR / "text_features.csv"
METADATA_PATH = PROCESSED_DATA_DIR / "text_metadata.json"

KNOWN_EMOTION_COLUMNS = [
    "admiration",
    "amusement",
    "anger",
    "annoyance",
    "approval",
    "caring",
    "confusion",
    "curiosity",
    "desire",
    "disappointment",
    "disapproval",
    "disgust",
    "embarrassment",
    "excitement",
    "fear",
    "gratitude",
    "grief",
    "joy",
    "love",
    "nervousness",
    "optimism",
    "pride",
    "realization",
    "relief",
    "remorse",
    "sadness",
    "surprise",
    "neutral",
]

POSITIVE_EMOTIONS = {
    "joy", "love", "admiration", "approval", "gratitude",
    "excitement", "optimism", "relief", "pride"
}

NEGATIVE_EMOTIONS = {
    "anger", "annoyance", "disappointment", "disapproval",
    "disgust", "embarrassment", "fear", "grief",
    "nervousness", "sadness", "remorse"
}


def clean_text(text: str) -> str:
    normalized = str(text).lower()
    normalized = re.sub(r"[^a-z0-9\s]", " ", normalized)
    tokens = [token for token in normalized.split() if token not in ENGLISH_STOP_WORDS]
    return " ".join(tokens)


def build_from_one_hot(df: pd.DataFrame, text_column: str) -> pd.DataFrame:
    available_emotions = [column for column in KNOWN_EMOTION_COLUMNS if column in df.columns]
    if not available_emotions:
        raise ValueError("No known GoEmotions one-hot label columns were found.")

    label_matrix = df[available_emotions].apply(pd.to_numeric, errors="coerce").fillna(0)
    records: list[dict[str, str]] = []

    for index, row in label_matrix.iterrows():
        active = [emotion for emotion in available_emotions if row[emotion] >= 1]

        if not active:
            continue

        cleaned = clean_text(str(df.at[index, text_column]))
        if not cleaned:
            continue

        if any(emotion in NEGATIVE_EMOTIONS for emotion in active):
            label = "negative"
        elif any(emotion in POSITIVE_EMOTIONS for emotion in active):
            label = "positive"
        else:
            continue

        records.append({
            "clean_text": cleaned,
            "sentiment": label
        })

    return pd.DataFrame.from_records(records)


def main() -> None:
    if not RAW_DATA_PATH.exists():
        raise FileNotFoundError(f"GoEmotions dataset not found at {RAW_DATA_PATH}")

    df = pd.read_csv(RAW_DATA_PATH)
    if "text" not in df.columns:
        raise ValueError("Expected a 'text' column in go_emotions_dataset.csv")

    print(f"Using text dataset: {RAW_DATA_PATH.name}")
    print(f"Dataset shape: {df.shape}")

    processed = build_from_one_hot(df, "text")
    processed = processed.replace({"sentiment": {"": pd.NA}}).dropna(subset=["clean_text", "sentiment"])
    processed = processed[processed["clean_text"].str.len() > 0].reset_index(drop=True)
    processed = processed[["clean_text", "sentiment"]]

    PROCESSED_DATA_DIR.mkdir(parents=True, exist_ok=True)
    processed.to_csv(PROCESSED_DATA_PATH, index=False)

    metadata = {
        "source_csv": RAW_DATA_PATH.name,
        "text_column": "text",
        "label_source": "goemotions_one_hot_to_sentiment",
        "row_count_after_cleaning": int(len(processed)),
        "class_distribution": processed["sentiment"].value_counts().to_dict(),
    }
    METADATA_PATH.write_text(json.dumps(metadata, indent=2), encoding="utf-8")

    print(f"Processed dataset shape: {processed.shape}")
    print("Sentiment distribution:")
    print(processed["sentiment"].value_counts())
    print(f"Saved processed text dataset to: {PROCESSED_DATA_PATH}")


if __name__ == "__main__":
    main()
