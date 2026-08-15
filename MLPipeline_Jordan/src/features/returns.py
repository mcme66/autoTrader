"""
Return and price-based feature engineering.
"""

import pandas as pd
import numpy as np


def add_return_features(df: pd.DataFrame) -> pd.DataFrame:
    """
    Add historical return and volatility features.

    All features use only information available on or before
    the observation date.
    """

    df = df.copy()

    df = df.sort_values(
        ["ticker", "date"]
    ).reset_index(drop=True)

    grouped = df.groupby("ticker", group_keys=False)

    # Historical returns
    df["return_1d"] = grouped["close"].pct_change(1)
    df["return_5d"] = grouped["close"].pct_change(5)
    df["return_20d"] = grouped["close"].pct_change(20)

    # 20-day rolling volatility of daily returns
    df["volatility_20d"] = (
        df.groupby("ticker")["return_1d"]
        .transform(lambda x: x.rolling(20).std())
    )

    return df