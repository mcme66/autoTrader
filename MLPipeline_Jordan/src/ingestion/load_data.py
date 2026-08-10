"""
Functions for loading stock market data into the ML pipeline.
"""

from pathlib import Path

import pandas as pd


def load_csv_data(file_path: str | Path) -> pd.DataFrame:
    """
    Load stock data from a CSV file.

    Parameters
    ----------
    file_path : str or Path
        Path to the CSV file.

    Returns
    -------
    pd.DataFrame
        Raw stock data.
    """

    file_path = Path(file_path)

    if not file_path.exists():
        raise FileNotFoundError(
            f"Data file not found: {file_path}"
        )

    return pd.read_csv(file_path)


def load_stock_data(
    source: str | Path,
    source_type: str = "csv"
) -> pd.DataFrame:
    """
    Load stock data from the specified source.

    Parameters
    ----------
    source : str or Path
        Location of the data source.

    source_type : str
        Type of data source. Currently supports 'csv'.

    Returns
    -------
    pd.DataFrame
        Raw stock data.
    """

    if source_type == "csv":
        return load_csv_data(source)

    raise ValueError(
        f"Unsupported source type: {source_type}"
    )