from __future__ import annotations

from flask import Flask, jsonify, request

from predict_behavior import predict_behavior
from predict_text import predict_text


app = Flask(__name__)


def build_message(prediction: str) -> str:
    if prediction == "low":
        return "You seem to be doing well today."
    if prediction == "moderate":
        return "Some imbalance detected."
    return "Your behavior indicates higher risk."


@app.post("/predict")
def predict() -> tuple:
    # Angular -> .NET -> Flask keeps the ML service isolated and easy to replace.
    # In .NET, call this Python API with:
    # await httpClient.PostAsJsonAsync("http://localhost:5000/predict", data);
    #
    # Angular should call your .NET endpoint and show the result in a popup/modal
    # on the same page, without navigating away.
    #
    # MongoDB is a good place to store:
    # sleep, exercise, food, prediction, timestamp
    #
    # This API is intentionally stateless:
    # - no retraining during runtime
    # - fast prediction only
    # - safe defaults for missing fields
    try:
        payload = request.get_json(silent=True)
        if payload is None:
            return jsonify({"error": "Request body must be valid JSON."}), 400
        if not isinstance(payload, dict):
            return jsonify({"error": "JSON payload must be an object."}), 400

        response = predict_behavior(payload)
        response["message"] = build_message(response["risk_level"])
        return jsonify(response), 200
    except FileNotFoundError as error:
        return jsonify({"error": str(error)}), 500
    except Exception as error:
        return jsonify({"error": f"Prediction failed: {error}"}), 500


@app.post("/predict-text")
def predict_text_endpoint() -> tuple:
    try:
        payload = request.get_json(silent=True)
        if payload is None:
            return jsonify({"error": "Request body must be valid JSON."}), 400
        if not isinstance(payload, dict):
            return jsonify({"error": "JSON payload must be an object."}), 400

        journal_text = payload.get("journalText") or payload.get("journal_text")
        if not journal_text or not str(journal_text).strip():
            return jsonify({"error": "journalText is required."}), 400

        response = predict_text(str(journal_text).strip())
        return jsonify(response), 200
    except ValueError as error:
        return jsonify({"error": str(error)}), 400
    except FileNotFoundError as error:
        return jsonify({"error": str(error)}), 500
    except Exception as error:
        return jsonify({"error": f"Prediction failed: {error}"}), 500


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=False)
