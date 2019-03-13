
package demo.view.dlineage.listener;

import demos.dlineage.dataflow.listener.DataFlowHandleListener;

public interface DataFlowUIListener extends DataFlowHandleListener
{

	public void startConvertXMLToModel( );

	public void endConvertXMLToModel( );

	public void startConvertModelToGraphXML( );

	public void endConvertModelToGraphXML( );

	public void startShowGraph( );

	public void startLoadGraphXML( );

	public void endLoadGraphXML( );

	public void startLayoutGraph( );

	public void endLayoutGraph( );

	public void startLayoutEdgeRouter( );

	public void endLayoutEdgeRouter( );

	public void endShowGraph( );
}
