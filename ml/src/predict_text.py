from __future__ import annotations

import json
import pickle
import re
from pathlib import Path
from typing import Any

from sklearn.feature_extraction.text import ENGLISH_STOP_WORDS


MODEL_DIR = Path(__file__).resolve().parent.parent / "models"
BEST_MODEL_PATH = MODEL_DIR / "text_best_model.pkl"
VECTORIZER_PATH = MODEL_DIR / "text_vectorizer.pkl"
LABEL_ENCODER_PATH = MODEL_DIR / "text_label_encoder.pkl"

COMPARISON_MODELS = {
    "logistic_regression": "text_logistic_regression.pkl",
    "naive_bayes": "text_naive_bayes.pkl",
}


def clean_text(text: str) -> str:
    normalized = str(text).lower()
    normalized = re.sub(r"[^a-z0-9\s]", " ", normalized)
    tokens = [token for token in normalized.split() if token not in ENGLISH_STOP_WORDS]
    return " ".join(tokens)


def load_common_artifacts() -> tuple[Any, Any]:
    with VECTORIZER_PATH.open("rb") as file:
        vectorizer = pickle.load(file)
    with LABEL_ENCODER_PATH.open("rb") as file:
        label_encoder = pickle.load(file)
    return vectorizer, label_encoder


def predict_with_model(model: Any, vectorizer: Any, label_encoder: Any, cleaned_text: str) -> dict[str, str]:
    vector = vectorizer.transform([cleaned_text])
    encoded = int(model.predict(vector)[0])
    sentiment = str(label_encoder.inverse_transform([encoded])[0])
    return {"sentiment": sentiment}


def predict_text(journal_text: str, comparison_mode: bool = False) -> dict[str, Any]:
    if not journal_text or not str(journal_text).strip():
        raise ValueError("Journal text is required.")

    cleaned = clean_text(journal_text)
    vectorizer, label_encoder = load_common_artifacts()

    if comparison_mode:
        outputs: dict[str, dict[str, str]] = {}
        for model_name, file_name in COMPARISON_MODELS.items():
            model_path = MODEL_DIR / file_name
            if not model_path.exists():
                continue
            with model_path.open("rb") as file:
                model = pickle.load(file)
            outputs[model_name] = predict_with_model(model, vectorizer, label_encoder, cleaned)
        return {"mode": "comparison", "clean_text": cleaned, "model_outputs": outputs}

    with BEST_MODEL_PATH.open("rb") as file:
        best_bundle = pickle.load(file)
    best_output = predict_with_model(best_bundle["model"], vectorizer, label_encoder, cleaned)
    return {"mode": "production", "selected_model": best_bundle["model_name"], **best_output}


def run_terminal_tests() -> None:
    samples = [
        "I feel exhausted and scared about everything lately.",
        "Today was okay, nothing special happened.",
        "I feel sad and lonely.",
    ]
    print("Text prediction test cases:")
    for index, sample in enumerate(samples, start=1):
        try:
            comparison = predict_text(sample, comparison_mode=True)
            production = predict_text(sample, comparison_mode=False)
            print(f"Test {index} text: {sample}")
            print(json.dumps(comparison))
            print(json.dumps(production))
            print("-" * 50)
        except Exception as error:
            print(f"Test {index} failed: {error}")
            print("-" * 50)


if __name__ == "__main__":
    run_terminal_tests()
