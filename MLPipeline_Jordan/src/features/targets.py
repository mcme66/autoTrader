"""
Target variables for supervised learning.
"""

import pandas as pd


def add_future_return_targets(
    df: pd.DataFrame,
    horizons: list[int] | None = None,
) -> pd.DataFrame:
    """
    Add future percentage-return targets.

    Parameters
    ----------
    df : pd.DataFrame
        Stock price data.

    horizons : list[int], optional
        Number of trading observations into the future.
        Defaults to 5, 15, and 30.

    Returns
    -------
    pd.DataFrame
        DataFrame with future-return target columns.
    """

    if horizons is None:
        horizons = [5, 15, 30]

    df = df.copy()

    df = df.sort_values(
        ["ticker", "date"]
    ).reset_index(drop=True)

    grouped = df.groupby("ticker", group_keys=False)

    for horizon in horizons:
        df[f"target_return_{horizon}d"] = (
            grouped["close"]
            .shift(-horizon)
            / df["close"]
            - 1
        )

    return df