"""
Validation functions for stock market datasets.

Each function raises a ValueError with a descriptive message if validation fails.
"""

import pandas as pd


REQUIRED_COLUMNS = [
    "date",
    "ticker",
    "open",
    "high",
    "low",
    "close",
    "volume",
]


def validate_columns(df: pd.DataFrame) -> None:
    """Ensure all required columns are present."""

    missing = [col for col in REQUIRED_COLUMNS if col not in df.columns]

    if missing:
        raise ValueError(
            f"Missing required columns: {', '.join(missing)}"
        )


def validate_dtypes(df: pd.DataFrame) -> None:
    """Check expected data types."""

    if not pd.api.types.is_datetime64_any_dtype(df["date"]):
        raise ValueError("'date' column must be datetime")

    numeric_cols = ["open", "high", "low", "close", "volume"]

    for col in numeric_cols:
        if not pd.api.types.is_numeric_dtype(df[col]):
            raise ValueError(f"'{col}' must be numeric")


def validate_missing_values(df: pd.DataFrame) -> None:
    """Ensure required columns contain no missing values."""

    missing = df[REQUIRED_COLUMNS].isnull().sum()

    missing = missing[missing > 0]

    if not missing.empty:
        raise ValueError(
            f"Missing values detected:\n{missing}"
        )


def validate_duplicate_rows(df: pd.DataFrame) -> None:
    """Ensure ticker/date combinations are unique."""

    duplicates = df.duplicated(
        subset=["ticker", "date"]
    )

    if duplicates.any():
        raise ValueError(
            f"Found {duplicates.sum()} duplicate ticker/date rows."
        )


def validate_prices(df: pd.DataFrame) -> None:
    """Validate OHLC price relationships."""

    if (df[["open", "high", "low", "close"]] <= 0).any().any():
        raise ValueError("Prices must all be greater than zero.")

    if (df["high"] < df[["open", "close"]].max(axis=1)).any():
        raise ValueError("High price is below open or close.")

    if (df["low"] > df[["open", "close"]].min(axis=1)).any():
        raise ValueError("Low price is above open or close.")


def validate_volume(df: pd.DataFrame) -> None:
    """Ensure trading volume is non-negative."""

    if (df["volume"] < 0).any():
        raise ValueError("Negative trading volume detected.")


def validate_dates(df: pd.DataFrame) -> None:
    """Ensure dates are sorted within each ticker."""

    for ticker, group in df.groupby("ticker"):

        if not group["date"].is_monotonic_increasing:
            raise ValueError(
                f"Dates for {ticker} are not sorted."
            )


def validate_dataframe(df: pd.DataFrame) -> None:
    """
    Run every validation step.

    Raises
    ------
    ValueError
        If any validation fails.
    """

    validate_columns(df)
    validate_dtypes(df)
    validate_missing_values(df)
    validate_duplicate_rows(df)
    validate_prices(df)
    validate_volume(df)
    validate_dates(df)