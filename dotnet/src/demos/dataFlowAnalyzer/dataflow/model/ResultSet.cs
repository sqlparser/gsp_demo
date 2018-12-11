using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using System;

    public class ResultSet
	{

		public static IDictionary<string, int?> DISPLAY_ID = new Dictionary<string, int?>();
		public static IDictionary<int?, string> DISPLAY_NAME = new Dictionary<int?, string>();

		private int id;
		private Tuple<long, long> startPosition;
		private Tuple<long, long> endPosition;
		private IList<ResultColumn> columns = new List<ResultColumn>();

		private TParseTreeNode gspObject;

		public ResultSet(TParseTreeNode gspObject)
		{
			if (gspObject == null)
			{
				throw new System.ArgumentException("ResultSet arguments can't be null.");
			}

			id = ++Table.TABLE_ID;

			this.gspObject = gspObject;

			TSourceToken startToken = gspObject.startToken;
			TSourceToken endToken = gspObject.endToken;

            if (startToken != null)
            {
                this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
            }
            else
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Can't get start token, the start token is null");
            }
            if (endToken != null)
            {
                this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);
            }
            else
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Can't get end token, the end token is null");
            }
        }

		public virtual Tuple<long, long> StartPosition
		{
			get
			{
				return startPosition;
			}
		}

		public virtual Tuple<long, long> EndPosition
		{
			get
			{
				return endPosition;
			}
		}

		public virtual IList<ResultColumn> Columns
		{
			get
			{
				return columns;
			}
		}

		public virtual void addColumn(ResultColumn column)
		{
			if (column != null && !columns.Contains(column))
			{
				this.columns.Add(column);
			}
		}

		public virtual TParseTreeNode GspObject
		{
			get
			{
				return gspObject;
			}
		}

		public virtual int Id
		{
			get
			{
				return id;
			}
		}

	}

}