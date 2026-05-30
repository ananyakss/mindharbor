from __future__ import annotations

import json
import pickle
from pathlib import Path

import pandas as pd
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score, confusion_matrix, f1_score
from sklearn.model_selection import train_test_split
from sklearn.naive_bayes import MultinomialNB
from sklearn.preprocessing import LabelEncoder


PROCESSED_DATA_PATH = Path(__file__).resolve().parent.parent / "data" / "processed" / "text_features.csv"
MODEL_DIR = Path(__file__).resolve().parent.parent / "models"
METRICS_PATH = MODEL_DIR / "text_metrics.json"
COMPARISON_PATH = MODEL_DIR / "text_comparison_results.json"


def evaluate(name: str, model: object, X_train, X_test, y_train, y_test, labels: list[int]) -> tuple[dict, object]:
    model.fit(X_train, y_train)
    predictions = model.predict(X_test)
    accuracy = accuracy_score(y_test, predictions)
    weighted_f1 = f1_score(y_test, predictions, average="weighted", zero_division=0)
    matrix = confusion_matrix(y_test, predictions, labels=labels).tolist()

    print(f"\n=== Text Model: {name} ===")
    print(f"Accuracy: {accuracy:.4f}")
    print(f"Weighted F1-score: {weighted_f1:.4f}")
    print("Confusion Matrix:")
    print(matrix)

    return {
        "model_name": name,
        "accuracy": accuracy,
        "weighted_f1": weighted_f1,
        "confusion_matrix": matrix,
    }, model


def main() -> None:
    if not PROCESSED_DATA_PATH.exists():
        raise FileNotFoundError(f"Processed text dataset missing: {PROCESSED_DATA_PATH}. Run build_text_dataset.py first.")

    df = pd.read_csv(PROCESSED_DATA_PATH)
    if "clean_text" not in df.columns or "sentiment" not in df.columns:
        raise ValueError("Processed text dataset must contain 'clean_text' and 'sentiment' columns.")

    X_text = df["clean_text"].astype(str)
    y_label = df["sentiment"].astype(str)

    label_encoder = LabelEncoder()
    y = label_encoder.fit_transform(y_label)

    X_train_text, X_test_text, y_train, y_test = train_test_split(
        X_text, y, test_size=0.2, random_state=42, stratify=y if len(set(y)) > 1 else None
    )

    vectorizer = TfidfVectorizer(
        max_features=30000,
        ngram_range=(1, 2),
        min_df=2,
        max_df=0.9,
    )
    X_train = vectorizer.fit_transform(X_train_text)
    X_test = vectorizer.transform(X_test_text)

    model_specs = {
        "logistic_regression": LogisticRegression(max_iter=3000, class_weight="balanced"),
        "naive_bayes": MultinomialNB(),
    }

    MODEL_DIR.mkdir(parents=True, exist_ok=True)
    labels = list(range(len(label_encoder.classes_)))
    results: list[dict] = []

    for name, model in model_specs.items():
        result, trained_model = evaluate(name, model, X_train, X_test, y_train, y_test, labels)
        results.append(result)
        with (MODEL_DIR / f"text_{name}.pkl").open("wb") as file:
            pickle.dump(trained_model, file)

    best = max(results, key=lambda row: (row["weighted_f1"], row["accuracy"]))
    best_name = best["model_name"]
    with (MODEL_DIR / f"text_{best_name}.pkl").open("rb") as file:
        best_model = pickle.load(file)

    with (MODEL_DIR / "text_best_model.pkl").open("wb") as file:
        pickle.dump({"model_name": best_name, "model": best_model}, file)
    with (MODEL_DIR / "text_vectorizer.pkl").open("wb") as file:
        pickle.dump(vectorizer, file)
    with (MODEL_DIR / "text_label_encoder.pkl").open("wb") as file:
        pickle.dump(label_encoder, file)

    metrics = {
        "task": "text_binary_sentiment",
        "results": results,
        "best_model": best_name,
        "label_classes": label_encoder.classes_.tolist(),
    }
    METRICS_PATH.write_text(json.dumps(metrics, indent=2), encoding="utf-8")
    COMPARISON_PATH.write_text(json.dumps(metrics, indent=2), encoding="utf-8")

    print("\n=== Text Comparison Summary ===")
    for row in results:
        print(f"{row['model_name']}: accuracy={row['accuracy']:.4f}, weighted_f1={row['weighted_f1']:.4f}")
    print(f"Best text model: {best_name}")


if __name__ == "__main__":
    main()
