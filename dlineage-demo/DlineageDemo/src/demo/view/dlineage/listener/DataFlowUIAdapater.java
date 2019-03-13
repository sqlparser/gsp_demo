
package demo.view.dlineage.listener;

import java.io.File;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;

import javax.swing.ProgressMonitor;

import demos.dlineage.util.SQLUtil;

public class DataFlowUIAdapater implements DataFlowUIListener
{

	private ProgressMonitor monitor;

	private long startAnalyzeTime;
	private long startParseTime;
	private long startDataFlowTime;
	private long startStatementTime;
	private long startToXMLTime;
	private long startToModelTime;
	private long startToGraphXMLTime;
	private long startShowGraphTime;
	private long startLoadGraphXMLTime;
	private long startLayoutGraphTime;
	private long startLayoutEdgeRouterTime;
	private long totalScriptCount;
	private long totalStatementCount;
	private int scriptStep;
	private int statementStep;
	private int progress;

	private File logFile;

	public DataFlowUIAdapater( ProgressMonitor monitor, File logFile )
	{
		this.monitor = monitor;
		this.logFile = logFile;
		clearLog( );
	}

	@Override
	public void startAnalyze( File file, long lengthOrCount, boolean isCount )
	{
		startAnalyzeTime = System.currentTimeMillis( );
		String time = formatLogTime( startAnalyzeTime );

		progress = 50000;
		monitor.setProgress( progress );

		if ( file != null )
		{
			if ( isCount )
			{
				totalScriptCount = lengthOrCount;
				String log = time
						+ "  Start analyzing directory "
						+ file.getName( )
						+ ", the file count is "
						+ lengthOrCount
						+ ".";
				appendLog( log );
				monitor.setNote( log );
			}
			else
			{
				totalScriptCount = 1;
				String log = time
						+ "  Start analyzing file "
						+ file.getName( )
						+ ", the file length is "
						+ lengthOrCount
						+ ".";
				appendLog( log );
				monitor.setNote( log );
			}
		}
		else
		{
			if ( isCount )
			{
				totalScriptCount = lengthOrCount;
				String log = time
						+ "  Start analyzing SQL scripts, the scripts count is "
						+ lengthOrCount
						+ ".";
				appendLog( log );
				monitor.setNote( log );
			}
			else
			{
				totalScriptCount = 1;
				String log = time
						+ "  Start analyzing SQL script , the script lenght is "
						+ lengthOrCount
						+ ".";
				appendLog( log );
				monitor.setNote( log );
			}
		}

		scriptStep = (int) ( 500000 / totalScriptCount / 2 );
	}

	@Override
	public void startParse( File file, long length, int index )
	{
		startParseTime = System.currentTimeMillis( );
		String time = formatLogTime( startParseTime );

		if ( file != null )
		{
			String log = time
					+ "  Start parsing the "
					+ ( totalScriptCount == 1 ? "" : ++index
							+ "th/"
							+ totalScriptCount )
					+ " file "
					+ file.getName( )
					+ ", the file length is "
					+ length
					+ ".";
			appendLog( log );
			monitor.setNote( log );
		}
		else
		{
			String log = time
					+ "  Start parsing SQL script, the script length is "
					+ length
					+ ".";
			monitor.setNote( log );
		}
	}

	@Override
	public void endParse( )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Parse SQL script finished, spend time "
				+ formatTime( currentTime - startParseTime )
				+ ".";
		appendLog( log );

		progress += scriptStep;
		monitor.setProgress( progress );
	}

	@Override
	public void startAnalyzeDataFlow( int totalCount )
	{
		totalStatementCount = totalCount;
		startDataFlowTime = System.currentTimeMillis( );
		String time = formatLogTime( startDataFlowTime );

		String log = time
				+ "  Start analyzing data flow, the statements count is "
				+ totalCount
				+ ".";
		appendLog( log );
		monitor.setNote( log );

		if ( totalCount > 0 )
		{
			statementStep = scriptStep / totalCount;
		}
	}

	@Override
	public void startAnalyzeStatment( int index )
	{
		startStatementTime = System.currentTimeMillis( );
		String time = formatLogTime( startStatementTime );

		String log = time
				+ "  Start analyzing the "
				+ ( totalStatementCount == 1 ? "" : ++index
						+ "th/"
						+ totalStatementCount )
				+ " statement.";
		appendLog( log );
		monitor.setNote( log );

		progress += statementStep;
		monitor.setProgress( progress );
	}

	@Override
	public void endAnalyzeStatment( int index )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Analyze the "
				+ ++index
				+ "th statement finished, spend time "
				+ formatTime( currentTime - startStatementTime )
				+ ".";
		appendLog( log );

	}

	@Override
	public void endAnalyzeDataFlow( )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Analyze data flow finished, spend time "
				+ formatTime( currentTime - startDataFlowTime )
				+ ".";
		appendLog( log );

		monitor.setProgress( 550000 );
	}

	@Override
	public void startOutputDataFlowXML( )
	{
		startToXMLTime = System.currentTimeMillis( );
		String time = formatLogTime( startToXMLTime );

		String log = time + "  Start outputing the data flow xml.";
		appendLog( log );
		monitor.setNote( log );

	}

	@Override
	public void endOutputDataFlowXML( long length )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Output the data flow xml finished, length is "
				+ length
				+ ", spend time "
				+ formatTime( currentTime - startToXMLTime )
				+ ".";
		appendLog( log );

		monitor.setProgress( 600000 );
	}

	@Override
	public void endAnalyze( )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Execute the data flow analyse finished, spend time "
				+ formatTime( currentTime - startAnalyzeTime )
				+ ".";
		appendLog( log );

		monitor.setProgress( 650000 );
	}

	@Override
	public void startConvertXMLToModel( )
	{
		startToModelTime = System.currentTimeMillis( );
		String time = formatLogTime( startToModelTime );

		String log = time + "  Start converting the data flow XML to UI model.";
		appendLog( log );
		monitor.setNote( log );
	}

	@Override
	public void endConvertXMLToModel( )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Convert the data flow XML to UI model finished, spend time "
				+ formatTime( currentTime - startToModelTime )
				+ ".";
		appendLog( log );

		monitor.setProgress( 700000 );
	}

	@Override
	public void startConvertModelToGraphXML( )
	{
		startToGraphXMLTime = System.currentTimeMillis( );
		String time = formatLogTime( startToGraphXMLTime );

		String log = time + "  Start converting the UI model to the graph XML.";
		appendLog( log );
		monitor.setNote( log );
	}

	@Override
	public void endConvertModelToGraphXML( )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Convert the UI model to the graph XML finished, spend time "
				+ formatTime( currentTime - startToModelTime )
				+ ".";
		appendLog( log );

		monitor.setProgress( 750000 );
	}

	@Override
	public void startShowGraph( )
	{
		startShowGraphTime = System.currentTimeMillis( );
		String time = formatLogTime( startShowGraphTime );

		String log = time + "  Start showing the graph.";
		appendLog( log );
		monitor.setNote( log );
	}

	@Override
	public void startLoadGraphXML( )
	{
		startLoadGraphXMLTime = System.currentTimeMillis( );
		String time = formatLogTime( startLoadGraphXMLTime );

		String log = time + "  Start loading the graph xml.";
		appendLog( log );
		monitor.setNote( log );
	}

	@Override
	public void endLoadGraphXML( )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Load the graph xml finished, spend time "
				+ formatTime( currentTime - startLoadGraphXMLTime )
				+ ".";
		appendLog( log );

		monitor.setProgress( 800000 );
	}

	@Override
	public void startLayoutGraph( )
	{
		startLayoutGraphTime = System.currentTimeMillis( );
		String time = formatLogTime( startLayoutGraphTime );

		String log = time + "  Start layouting the graph.";
		appendLog( log );
		monitor.setNote( log );

	}

	@Override
	public void endLayoutGraph( )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Layout the graph finished, spend time "
				+ formatTime( currentTime - startLayoutGraphTime )
				+ ".";
		appendLog( log );
		monitor.setProgress( 900000 );
	}

	@Override
	public void startLayoutEdgeRouter( )
	{
		startLayoutEdgeRouterTime = System.currentTimeMillis( );
		String time = formatLogTime( startLayoutEdgeRouterTime );

		String log = time + "  Start layouting the edge router.";
		appendLog( log );
		monitor.setNote( log );
	}

	@Override
	public void endLayoutEdgeRouter( )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Layout the edge router finished, spend time "
				+ formatTime( currentTime - startLayoutEdgeRouterTime )
				+ ".";
		appendLog( log );
		monitor.setProgress( 990000 );
	}

	@Override
	public void endShowGraph( )
	{
		long currentTime = System.currentTimeMillis( );
		String time = formatLogTime( currentTime );

		String log = time
				+ "  Show the graph finished, spend time "
				+ formatTime( currentTime - startShowGraphTime )
				+ ".";
		appendLog( log );
		monitor.setProgress( monitor.getMaximum( ) );
	}

	@Override
	public boolean isCanceled( )
	{
		return monitor.isCanceled( );
	}

	private String formatLogTime( long time )
	{
		SimpleDateFormat df = new SimpleDateFormat( "yyyy-MM-dd HH:mm:ss" );
		return df.format( new Date( time ) );
	}

	private void appendLog( String log )
	{
		if ( logFile != null && log != null )
		{
			try
			{
				SQLUtil.appendToFile( logFile, log );
			}
			catch ( IOException e )
			{
				e.printStackTrace( );
			}
		}
		else
		{
			System.out.println( log );
		}
	}

	private String formatTime( long time )
	{
		if ( time > 1000 )
		{
			return time / 1000 + " seconds";
		}
		else
		{
			return time + " ms";
		}
	}

	private void clearLog( )
	{
		if ( logFile != null )
		{
			logFile.delete( );
		}
	}

}
