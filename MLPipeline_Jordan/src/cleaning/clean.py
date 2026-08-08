"""
Cleaning functions for stock market datasets.
"""

import pandas as pd


NUMERIC_COLUMNS = [
    "open",
    "high",
    "low",
    "close",
    "volume",
]


def standardize_column_names(df: pd.DataFrame) -> pd.DataFrame:
    """Standardize column names to lowercase snake_case."""

    df = df.copy()

    df.columns = (
        df.columns
        .str.strip()
        .str.lower()
        .str.replace(" ", "_")
    )

    return df


def standardize_data_types(df: pd.DataFrame) -> pd.DataFrame:
    """Convert columns to the expected data types."""

    df = df.copy()

    df["date"] = pd.to_datetime(df["date"])

    for column in NUMERIC_COLUMNS:
        df[column] = pd.to_numeric(df[column], errors="coerce")

    df["ticker"] = df["ticker"].astype(str).str.strip().str.upper()

    return df


def sort_stock_data(df: pd.DataFrame) -> pd.DataFrame:
    """Sort stock data chronologically within each ticker."""

    df = df.copy()

    return df.sort_values(
        by=["ticker", "date"]
    ).reset_index(drop=True)


def clean_stock_data(df: pd.DataFrame) -> pd.DataFrame:
    """
    Apply all standard cleaning operations to stock data.
    """

    df = standardize_column_names(df)
    df = standardize_data_types(df)
    df = sort_stock_data(df)

    return df