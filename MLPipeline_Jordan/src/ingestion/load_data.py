"""
Functions for ingesting stock market data into the ML pipeline.
"""

from pathlib import Path

import pandas as pd
from sqlalchemy import create_engine, text
from sqlalchemy.engine import Engine


def load_csv_data(file_path: str | Path) -> pd.DataFrame:
    """
    Load stock data from a CSV file.
    """

    file_path = Path(file_path)

    if not file_path.exists():
        raise FileNotFoundError(
            f"Data file not found: {file_path}"
        )

    if file_path.suffix.lower() != ".csv":
        raise ValueError(
            f"Expected a CSV file, received: {file_path.suffix}"
        )

    return pd.read_csv(file_path)


def create_postgres_engine(
    database_url: str
) -> Engine:
    """
    Create a SQLAlchemy engine for PostgreSQL.
    """

    if not database_url:
        raise ValueError(
            "A PostgreSQL database URL is required."
        )

    return create_engine(database_url)


def load_postgres_data(
    engine: Engine,
    query: str,
    params: dict | None = None
) -> pd.DataFrame:
    """
    Execute a PostgreSQL query and return the results
    as a pandas DataFrame.
    """

    if not query.strip():
        raise ValueError("SQL query cannot be empty.")

    with engine.connect() as connection:
        return pd.read_sql(
            text(query),
            connection,
            params=params
        )


def load_stock_data(
    source,
    source_type: str = "csv",
    query: str | None = None,
) -> pd.DataFrame:
    """
    Load stock data from the specified source.

    Currently supports CSV and PostgreSQL.
    """

    if source_type == "csv":
        return load_csv_data(source)

    if source_type == "postgres":
        if query is None:
            raise ValueError(
                "A SQL query is required for PostgreSQL."
            )

        return load_postgres_data(
            engine=source,
            query=query
        )

    raise ValueError(
        f"Unsupported source type: {source_type}"
    )