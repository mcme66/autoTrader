## Overall pipeline:
- Start
- Load Configuration
- Connect to PostgreSQL
- Read Required Tables
- Clean / Validate Data
- Engineer Features
- Load Production Model
- Generate Predictions
- Generate Trading Signal
- Estimate Confidence
- Write to ml_predictions
- Log Success

### Training Pipeline:
- Historical Data
- Feature Engineering
- Split Data
- Train Model
- Evaluate
- Save Model
- Update ml_models

### Evaluation Pipeline
- Previous Predictions
- Wait Until Target Date
- Read Actual Prices
- Compute Error
- Write ml_prediction_history

### Feature Engineering Stage:
- Raw Market Data
- Remove Missing Values
- Compute Returns
- Compute Moving Averages
- Compute Momentum
- Compute Volatility
- Normalize Features
- Prediction Matrix

