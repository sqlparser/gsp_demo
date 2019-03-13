
package demo.view.dlineage.painter;

import y.view.EdgeRealizer;
import y.view.PolyLineEdgeRealizer;

public class CustomEdgeRealizer extends PolyLineEdgeRealizer
{

	public CustomEdgeRealizer( EdgeRealizer oldRealizer )
	{
		super( oldRealizer );
	}

	public double getArrowScaleFactor( )
	{
		return 0.67;
	}
}
