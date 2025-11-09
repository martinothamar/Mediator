# /// script
# dependencies = ["pandas", "tabulate"]
# requires-python = ">=3.14"
# ///
import pandas as pd
import sys

# Small script to compare "Comparison" benchmark results.
# Since the result set is pretty large, it's useful to merge before and after
# tables to compute some differences and make it easier to read.
# Usage:
#   uv run cmp.py before-results.csv after-results.csv

if __name__ == '__main__':
    main = pd.read_csv(sys.argv[1], sep=',')
    pr = pd.read_csv(sys.argv[2], sep=',')

    # print("Columns in the joined DataFrame (single line):")
    # print("Current:", current.columns)

    # if len(pr.columns) != len(current.columns):
    #     print("Columns in the joined DataFrame are not equal")
    #     current_columns = set(current.columns)
    #     pr_columns = set(pr.columns)
    #     print("Columns in 'current' but not in 'pr':", current_columns - pr_columns)
    #     print("Columns in 'pr' but not in 'current':", pr_columns - current_columns)

    keep_columns = ['Method', 'ServiceLifetime', 'Project type', 'Categories', 'Mean', 'Error', 'StdDev', 'Ratio', 'RatioSD', 'Rank', 'Allocated', 'Alloc Ratio']
    join_on = ['Method', 'Categories', 'ServiceLifetime','Project type']
    metric_columns = set(keep_columns) - set(join_on)

    main = main[keep_columns]
    pr = pr[keep_columns]

    main_suffix = ' Main'
    pr_suffix = ' PR'
    df = pd.merge(main, pr, on=join_on, how='inner', suffixes=(main_suffix, pr_suffix))

    drop_cols_with_prefix = ['Ratio', 'RatioSD', 'Alloc Ratio']
    for col in drop_cols_with_prefix:
        df = df.loc[:, ~df.columns.str.startswith(col)]

    df["Rank Main"] = df["Rank Main"].astype(int)
    df["Rank PR"] = df["Rank PR"].astype(int)

    df["Mean Main"] = df["Mean Main"].str.replace(" ns", "", regex=False).astype(float)
    df["Mean PR"] = df["Mean PR"].str.replace(" ns", "", regex=False).astype(float)
    df["Allocated Main"] = df["Allocated Main"].str.replace(" B", "", regex=False).astype(float)
    df["Allocated PR"] = df["Allocated PR"].str.replace(" B", "", regex=False).astype(float)

    df["Mean improvement"] = (df["Mean PR"] - df["Mean Main"]).map(lambda x: f"{x:.3f} ns")
    df.loc[df["Method"].str.contains("MediatR"), 'Mean improvement'] = ""

    df['Ratio (Main -> PR)'] = (df["Mean Main"] / df["Mean PR"]).map(lambda x: f"{x:.2f}x {x > 1.0 and 'faster' or 'slower'}")
    df.loc[df['Ratio (Main -> PR)'].str.contains("slower"), 'Ratio (Main -> PR)'] = (df["Mean PR"] / df["Mean Main"]).map(lambda x: f"{x:.2f}x slower")

    df.loc[df["Method"].str.contains("MediatR"), 'Ratio (Main -> PR)'] = ""
    df["Allocated improvement"] = (df["Allocated PR"] - df["Allocated Main"]).map(lambda x: f"{x:.3f} B")
    df.loc[df["Method"].str.contains("MediatR"), "Allocated improvement"] = ""

    df["Allocated Ratio (Main -> PR)"] = (df["Allocated Main"] / df["Allocated PR"]).map(lambda x: f"{x:.2f}x {x > 1.0 and 'less' or 'more'}")
    df.loc[df["Allocated Ratio (Main -> PR)"].str.contains("more"), "Allocated Ratio (Main -> PR)"] = (df["Allocated PR"] / df["Allocated Main"]).map(lambda x: f"{x:.2f}x more")
    df.loc[df["Allocated Ratio (Main -> PR)"].str.contains("nanx"), "Allocated Ratio (Main -> PR)"] = ""
    df.loc[df["Allocated Ratio (Main -> PR)"].str.contains("infx"), "Allocated Ratio (Main -> PR)"] = ""

    df.loc[df["Method"].str.contains("MediatR"), 'Allocated Ratio (Main -> PR)'] = ""

    sort_columns_by_prefix = ['Rank', 'Mean', 'Ratio', 'Allocated', 'Error', 'StdDev']
    # Sort columns by prefix
    for col in sort_columns_by_prefix:
        sorted_cols = sorted(df.columns[df.columns.str.startswith(col)])
        df = df[df.columns[~df.columns.str.startswith(col)].tolist() + sorted_cols]
    df = df[join_on + df.columns[len(join_on):].tolist()]

    # If we want to ignore the MediatR results
    # df = df[~df['Method'].str.contains('MediatR', na=False)]

    df = df.sort_values(['Categories', 'ServiceLifetime', 'Project type', 'Mean PR'], ascending=[True, True, False, True])

    grps = df.groupby(['Categories', 'ServiceLifetime', 'Project type'], sort=False)
    df = pd.concat(
        [
            pd.concat([g, pd.DataFrame("", index=range(1), columns=df.columns)])
            if i < len(grps)-1 else g for i, (_, g) in enumerate(grps)
        ]
    )

    print(df.to_markdown(index=False))

    # Get average mean improvement in percentage for all non-empty and non-na rows
    df = df.dropna(subset=['Mean Main', 'Mean PR'])
    df = df[df['Mean Main'] != '']
    df = df[df['Mean PR'] != '']
    avg_mean_impr_perc = (((df['Mean Main'] - df['Mean PR']) / df['Mean Main']) * 100).mean()
    print()
    print(f"Average mean improvement: {avg_mean_impr_perc:.2f} %")
