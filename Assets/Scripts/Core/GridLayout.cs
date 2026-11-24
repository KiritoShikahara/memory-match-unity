namespace MemoryMatch.Core
{
    public readonly struct GridFit
    {
        public GridFit(int columns, float cardWidth, float cardHeight)
        {
            Columns = columns;
            CardWidth = cardWidth;
            CardHeight = cardHeight;
        }

        public int Columns { get; }
        public float CardWidth { get; }
        public float CardHeight { get; }
    }

    /// 枚数を領域に収め、カードが最大になる列数を選ぶ。aspect = 幅/高さ。
    public static class GridLayout
    {
        public static GridFit Fit(int count, float width, float height, float aspect, float gap)
        {
            var best = new GridFit(1, 0f, 0f);
            for (var cols = 1; cols <= count; cols++)
            {
                var rows = (count + cols - 1) / cols;
                var byWidth = (width - (cols - 1) * gap) / cols;
                var byHeight = (height - (rows - 1) * gap) / rows * aspect;
                var cardWidth = byWidth < byHeight ? byWidth : byHeight;
                if (cardWidth > best.CardWidth) best = new GridFit(cols, cardWidth, cardWidth / aspect);
            }
            return best;
        }
    }
}
