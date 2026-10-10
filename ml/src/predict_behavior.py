from __future__ import annotations

import json
import pickle
from pathlib import Path
from typing import Any

import pandas as pd

MODEL_DIR = Path(__file__).resolve().parent.parent / "models"
BEST_BUNDLE_PATH = MODEL_DIR / "behavior_best_model.pkl"
BEST_SCALER_PATH = MODEL_DIR / "behavior_best_scaler.pkl"
COMPARISON_MODELS = {
    "logistic_regression": ("behavior_logistic_regression.pkl", "behavior_logistic_regression_scaler.pkl"),
    "decision_tree": ("behavior_decision_tree.pkl", None),
    "random_forest": ("behavior_random_forest.pkl", None),
}


def load_best_artifacts() -> tuple[Any, Any, list[str], str]:
    if not BEST_BUNDLE_PATH.exists():
        raise FileNotFoundError("Best behavioral model not found. Run train_behavior_models.py first.")
    with BEST_BUNDLE_PATH.open("rb") as file:
        bundle = pickle.load(file)
    scaler = None
    if BEST_SCALER_PATH.exists():
        with BEST_SCALER_PATH.open("rb") as file:
            scaler = pickle.load(file)
    return bundle["model"], scaler, bundle["feature_columns"], bundle["model_name"]


def safe_number(value: Any, minimum: float, maximum: float, default: float = 0.0) -> float:
    if value is None:
        return default
    if isinstance(value, (int, float)):
        numeric = float(value)
        return min(max(numeric, minimum), maximum)

    text = str(value).strip().lower()
    if text in {"", "none", "null", "missing"}:
        return default
    try:
        numeric = float(text)
        return min(max(numeric, minimum), maximum)
    except ValueError:
        return default


def map_behavior_input(input_data: dict[str, Any]) -> dict[str, float]:
    mapped = {
        "sleep": safe_number(input_data.get("sleep"), 1, 10, default=7),
        "steps": safe_number(input_data.get("steps"), 0, 10, default=5),
        "diet": safe_number(input_data.get("diet"), 0, 5, default=2),
        "social": safe_number(input_data.get("social"), 0, 10, default=3),
        "hobbies": safe_number(input_data.get("hobbies"), 0, 10, default=2),
        "stress": safe_number(input_data.get("stress"), 1, 5, default=3),
    }
    mapped["low_sleep"] = 1 if mapped["sleep"] < 5 else 0
    mapped["low_social"] = 1 if mapped["social"] < 3 else 0
    mapped["inactive"] = 1 if mapped["steps"] < 3 else 0
    mapped["healthy_diet"] = 1 if mapped["diet"] >= 3 else 0
    mapped["high_hobby_time"] = 1 if mapped["hobbies"] >= 5 else 0
    mapped["high_stress"] = 1 if mapped["stress"] >= 4 else 0
    return mapped


def predict_single_model(model: Any, scaler: Any, feature_columns: list[str], mapped_input: dict[str, float]) -> str:
    frame = pd.DataFrame([[mapped_input[column] for column in feature_columns]], columns=feature_columns)
    model_input = scaler.transform(frame) if scaler is not None else frame
    prediction = str(model.predict(model_input)[0]).strip().lower()
    return prediction


def predict_behavior(input_data: dict[str, Any], comparison_mode: bool = False) -> dict[str, Any]:
    mapped_input = map_behavior_input(input_data)
    if comparison_mode:
        outputs: dict[str, dict[str, Any]] = {}
        feature_columns = [
            "sleep", "steps", "diet", "social", "hobbies", "stress",
            "low_sleep", "low_social", "inactive", "healthy_diet", "high_hobby_time", "high_stress",
        ]
        for model_name, (model_file, scaler_file) in COMPARISON_MODELS.items():
            model_path = MODEL_DIR / model_file
            if not model_path.exists():
                continue
            with model_path.open("rb") as file:
                model = pickle.load(file)
            scaler = None
            if scaler_file:
                scaler_path = MODEL_DIR / scaler_file
                if scaler_path.exists():
                    with scaler_path.open("rb") as file:
                        scaler = pickle.load(file)
            risk_level = predict_single_model(model, scaler, feature_columns, mapped_input)
            outputs[model_name] = {"risk_level": risk_level}
        return {"input_used": mapped_input, "mode": "comparison", "model_outputs": outputs}

    model, scaler, feature_columns, model_name = load_best_artifacts()
    risk_level = predict_single_model(model, scaler, feature_columns, mapped_input)
    return {
        "input_used": mapped_input,
        "mode": "production",
        "selected_model": model_name,
        "risk_level": risk_level,
    }


def run_terminal_tests() -> None:
    test_cases = [
        {"sleep": 0, "steps": 0, "diet": 0, "social": 0, "hobbies": 0},
        {"sleep": 8, "steps": 8, "diet": 5, "social": 8, "hobbies": 7},
        {"sleep": 6, "steps": 4, "diet": 2, "social": 4, "hobbies": 3},
    ]

    print("Behavior prediction test cases:")
    for index, test_case in enumerate(test_cases, start=1):
        try:
            comparison_response = predict_behavior(test_case, comparison_mode=True)
            production_response = predict_behavior(test_case, comparison_mode=False)
            print(f"Test {index} input: {test_case}")
            print(f"Comparison output: {json.dumps(comparison_response)}")
            print(f"Production output: {json.dumps(production_response)}")
            print("-" * 50)
        except Exception as error:
            print(f"Test {index} failed: {error}")
            print("-" * 50)


if __name__ == "__main__":
    run_terminal_tests()
