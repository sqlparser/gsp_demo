
package demos.dlineage.dataflow.model;

import demos.dlineage.util.Pair;
import gudusoft.gsqlparser.EExpressionType;
import gudusoft.gsqlparser.TSourceToken;
import gudusoft.gsqlparser.nodes.TObjectName;
import gudusoft.gsqlparser.nodes.TResultColumn;
import gudusoft.gsqlparser.nodes.TResultColumnList;

public class SelectSetResultColumn extends ResultColumn
{

	public SelectSetResultColumn( ResultSet resultSet,
			TResultColumn resultColumnObject )
	{
		super( resultSet, resultColumnObject );

		if ( resultColumnObject.getAliasClause( ) != null )
		{
			this.name = resultColumnObject.getAliasClause( ).toString( );
		}
		else
		{
			if ( resultColumnObject.getExpr( ).getExpressionType( ) == EExpressionType.simple_constant_t )
			{
				if ( resultSet instanceof SelectResultSet )
				{
					this.name = "DUMMY"
							+ getIndexOf( ( (SelectResultSet) resultSet ).getResultColumnObject( ),
									resultColumnObject );
				}
				else if ( resultSet instanceof SelectSetResultSet )
				{
					this.name = "DUMMY"
							+ getIndexOf( ( (SelectSetResultSet) resultSet ).getResultColumnObject( ),
									resultColumnObject );
				}
				else
					this.name = resultColumnObject.getColumnNameOnly( );
			}
			else
				this.name = resultColumnObject.getColumnNameOnly( );
		}

		this.fullName = this.name;
	}

	public SelectSetResultColumn( ResultSet resultSet, ResultColumn resultColumn )
	{
		if ( resultColumn == null || resultSet == null )
			throw new IllegalArgumentException( "ResultColumn arguments can't be null." );

		id = ++TableColumn.TABLE_COLUMN_ID;

		this.resultSet = resultSet;
		resultSet.addColumn( this );

		if ( resultColumn.getColumnObject( ) instanceof TResultColumn )
		{
			TResultColumn resultColumnObject = (TResultColumn) resultColumn.getColumnObject( );
			if ( resultColumnObject.getAliasClause( ) != null )
			{
				this.alias = resultColumnObject.getAliasClause( ).toString( );
				TSourceToken startToken = resultColumnObject.getAliasClause( )
						.getStartToken( );
				TSourceToken endToken = resultColumnObject.getAliasClause( )
						.getEndToken( );
				this.aliasStartPosition = new Pair<Long, Long>( startToken.lineNo,
						startToken.columnNo );
				this.aliasEndPosition = new Pair<Long, Long>( endToken.lineNo,
						endToken.columnNo + endToken.astext.length( ) );

				this.name = this.alias;
			}
			else
			{
				if ( resultColumnObject.getExpr( ).getExpressionType( ) == EExpressionType.simple_constant_t )
				{
					if ( resultSet instanceof SelectResultSet )
					{
						this.name = "DUMMY"
								+ getIndexOf( ( (SelectResultSet) resultSet ).getResultColumnObject( ),
										resultColumnObject );
					}
					else if ( resultSet instanceof SelectSetResultSet )
					{
						this.name = "DUMMY"
								+ getIndexOf( ( (SelectSetResultSet) resultSet ).getResultColumnObject( ),
										resultColumnObject );
					}
					else
						this.name = resultColumnObject.toString( );

				}
				else if ( resultColumnObject.getExpr( ).getExpressionType( ) == EExpressionType.function_t )
				{
					this.name = resultColumnObject.getExpr( )
							.getFunctionCall( )
							.getFunctionName( )
							.toString( );
				}
				else if ( resultColumnObject.getColumnNameOnly( ) != null
						&& !"".equals( resultColumnObject.getColumnNameOnly( ) ) )
				{
					this.name = resultColumnObject.getColumnNameOnly( );
				}
				else
				{
					this.name = resultColumnObject.toString( );
				}
			}

			if ( resultColumnObject.getExpr( ).getExpressionType( ) == EExpressionType.function_t )
			{
				this.fullName = resultColumnObject.getExpr( )
						.getFunctionCall( )
						.getFunctionName( )
						.toString( );
			}
			else
			{
				this.fullName = resultColumnObject.toString( );
			}

			TSourceToken startToken = resultColumnObject.getStartToken( );
			TSourceToken endToken = resultColumnObject.getEndToken( );
			this.startPosition = new Pair<Long, Long>( startToken.lineNo,
					startToken.columnNo );
			this.endPosition = new Pair<Long, Long>( endToken.lineNo,
					endToken.columnNo + endToken.astext.length( ) );
			this.columnObject = resultColumnObject;
		}
		else if ( resultColumn.getColumnObject( ) instanceof TObjectName )
		{
			TObjectName resultColumnObject = (TObjectName) resultColumn.getColumnObject( );

			if ( resultColumnObject.getColumnNameOnly( ) != null
					&& !"".equals( resultColumnObject.getColumnNameOnly( ) ) )
			{
				this.name = resultColumnObject.getColumnNameOnly( );
			}
			else
			{
				this.name = resultColumnObject.toString( );
			}

			this.fullName = this.name;

			TSourceToken startToken = resultColumnObject.getStartToken( );
			TSourceToken endToken = resultColumnObject.getEndToken( );
			this.startPosition = new Pair<Long, Long>( startToken.lineNo,
					startToken.columnNo );
			this.endPosition = new Pair<Long, Long>( endToken.lineNo,
					endToken.columnNo + endToken.astext.length( ) );
			this.columnObject = resultColumnObject;
		}
		else
		{
			this.name = resultColumn.getName( );
			this.fullName = this.name;

			TSourceToken startToken = resultColumn.getColumnObject( )
					.getStartToken( );
			TSourceToken endToken = resultColumn.getColumnObject( )
					.getEndToken( );
			this.startPosition = new Pair<Long, Long>( startToken.lineNo,
					startToken.columnNo );
			this.endPosition = new Pair<Long, Long>( endToken.lineNo,
					endToken.columnNo + endToken.astext.length( ) );
			this.columnObject = resultColumn.getColumnObject( );
		}
	}

	private int getIndexOf( TResultColumnList resultColumnList,
			TResultColumn resultColumnObject )
	{
		for ( int i = 0; i < resultColumnList.size( ); i++ )
		{
			if ( resultColumnList.getResultColumn( i ) == resultColumnObject )
				return i;
		}
		return -1;
	}
}
