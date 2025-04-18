import pandas as pd

# Small script to compare "Comparison" benchmark results.
# Since the result set is pretty large, it's useful to merge before and after
# tables to compute some differences and make it easier to read.

if __name__ == '__main__':
    current = pd.read_csv("compare/1Current.csv", sep=',')
    pr = pd.read_csv("compare/1PR.csv", sep=',')

    print("Columns in the joined DataFrame (single line):")
    print("Current:", current.columns)

    if len(pr.columns) != len(current.columns):
        print("Columns in the joined DataFrame are not equal")
        current_columns = set(current.columns)
        pr_columns = set(pr.columns)
        print("Columns in 'current' but not in 'pr':", current_columns - pr_columns)
        print("Columns in 'pr' but not in 'current':", pr_columns - current_columns)

    keep_columns = ['Method', 'ServiceLifetime', 'Project type', 'Categories', 'Mean', 'Error', 'StdDev', 'Ratio', 'RatioSD', 'Rank', 'Allocated', 'Alloc Ratio']
    join_on = ['Method', 'Categories', 'ServiceLifetime','Project type']
    metric_columns = set(keep_columns) - set(join_on)

    current = current[keep_columns]
    pr = pr[keep_columns]

    joined = pd.merge(current, pr, on=join_on, how='inner', suffixes=('_cu', '_pr'))

    joined['Rank_cu'] = joined['Rank_cu'].astype(int)
    joined['Rank_pr'] = joined['Rank_pr'].astype(int)

    joined['Mean_cu'] = joined['Mean_cu'].str.replace(' ns', '', regex=False).astype(float)
    joined['Mean_pr'] = joined['Mean_pr'].str.replace(' ns', '', regex=False).astype(float)
    joined['Allocated_cu'] = joined['Allocated_cu'].str.replace(' B', '', regex=False).astype(float)
    joined['Allocated_pr'] = joined['Allocated_pr'].str.replace(' B', '', regex=False).astype(float)

    joined['Rank_zgained'] = (joined['Rank_cu'] - joined['Rank_pr'])
    joined['Mean_zimpr'] = joined['Mean_pr'] - joined['Mean_cu']
    joined["Mean_zimpr_%"] = (((joined['Mean_cu'] - joined['Mean_pr']) / joined['Mean_cu']) * 100).map("{:.2f}%".format)
    joined['Allocated_zimpr'] = joined['Allocated_pr'] - joined['Allocated_cu']
    joined["Allocated_zimpr_%"] = (((joined['Allocated_cu'] - joined['Allocated_pr']) / joined['Allocated_cu']) * 100).map("{:.2f}%".format)

    drop_cols_with_prefix = ['Ratio', 'RatioSD', 'Alloc Ratio']
    for col in drop_cols_with_prefix:
        joined = joined.loc[:, ~joined.columns.str.startswith(col)]

    sort_columns_by_prefix = ['Rank', 'Mean', 'Allocated', 'Error', 'StdDev']
    # Sort columns by prefix
    for col in sort_columns_by_prefix:
        sorted_cols = sorted(joined.columns[joined.columns.str.startswith(col)])
        joined = joined[joined.columns[~joined.columns.str.startswith(col)].tolist() + sorted_cols]

    joined = joined[join_on + joined.columns[len(join_on):].tolist()]
    joined.columns = joined.columns.str.replace(r'_z', '_')

    joined = joined[~joined['Method'].str.contains('MediatR', na=False)]

    joined = joined.sort_values(by=['Mean_impr'], ascending=True)

    print("Whole dataset:")
    print(joined)

    groups = joined.groupby(['Project type'])
    for key, group in groups:
        print(f"\nGroup: {key}")
        print(group)



