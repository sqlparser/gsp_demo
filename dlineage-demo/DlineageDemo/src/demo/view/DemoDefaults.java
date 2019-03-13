/****************************************************************************
 * This demo file is part of yFiles for Java 2.12.
 * Copyright (c) 2000-2015 by yWorks GmbH, Vor dem Kreuzberg 28,
 * 72070 Tuebingen, Germany. All rights reserved.
 * 
 * yFiles demo files exhibit yFiles for Java functionalities. Any redistribution
 * of demo files in source code or binary form, with or without
 * modification, is not permitted.
 * 
 * Owners of a valid software license for a yFiles for Java version that this
 * demo is shipped with are allowed to use the demo source code as basis
 * for their own yFiles for Java powered applications. Use of such programs is
 * governed by the rights and conditions as set out in the yFiles for Java
 * license agreement.
 * 
 * THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 * NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 * TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 ***************************************************************************/

package demo.view;

import java.awt.Color;
import java.awt.Graphics2D;
import java.util.Map;

import y.base.Node;
import y.base.NodeCursor;
import y.geom.OrientedRectangle;
import y.view.Arrow;
import y.view.GenericNodeRealizer;
import y.view.Graph2D;
import y.view.Graph2DView;
import y.view.NodeLabel;
import y.view.NodeRealizer;
import y.view.PolyLineEdgeRealizer;
import y.view.ShinyPlateNodePainter;
import y.view.GenericNodeRealizer.Factory;
import y.view.SmartNodeLabelModel;

import javax.swing.UIManager;

/**
 * Provides default node and edge realizer configurations used by most demos.
 */
public class DemoDefaults
{

	/**
	 * The Name of the GenericNodeRealizer configuration of the default node
	 * used by most demos.
	 */
	public static final String NODE_CONFIGURATION = "DemoDefaults#Node";

	/**
	 * The default node fill color used by most demos
	 */
	public static final Color DEFAULT_NODE_COLOR = new Color( 255, 153, 0 );

	/**
	 * The default node line color used by most demos. This is set to
	 * <code>null</code> meaning no border is drawn.
	 */
	public static final Color DEFAULT_NODE_LINE_COLOR = null;

	/**
	 * The default secondary or contract color used by most demos
	 */
	public static final Color DEFAULT_CONTRAST_COLOR = new Color( 202, 227, 255 );

	private DemoDefaults( )
	{
	}

	static
	{
		registerDefaultNodeConfiguration( true );
	}

	/**
	 * Registers the default node configuration for yFiles demo applications.
	 * This method is called automatically when the <code>DemoDefaults</code>
	 * class is initialized.
	 * 
	 * @param drawShadows
	 *            if <code>true</code>, a drop shadow is drawn for nodes that
	 *            use the default configuration; otherwise no shadow is drawn.
	 */
	public static void registerDefaultNodeConfiguration( boolean drawShadows )
	{
		Factory factory = GenericNodeRealizer.getFactory( );
		Map configurationMap = factory.createDefaultConfigurationMap( );

		ShinyPlateNodePainter painter = new ShinyPlateNodePainter( );
		// ShinyPlateNodePainter has an option to draw a drop shadow that is
		// more efficient
		// than wrapping it in a ShadowNodePainter.
		painter.setDrawShadow( drawShadows );

		configurationMap.put( GenericNodeRealizer.Painter.class, painter );
		configurationMap.put( GenericNodeRealizer.ContainsTest.class, painter );
		factory.addConfiguration( NODE_CONFIGURATION, configurationMap );
	}

	/**
	 * Configures the default node and edge realizer of the specified view's
	 * associated graph.
	 * <p>
	 * The default representation used for a node is provided by a
	 * {@link y.view.GenericNodeRealizer} that uses the configuration mapped to
	 * {@link #NODE_CONFIGURATION}. The default colors (fill, border) for a node
	 * are set to {@link #DEFAULT_NODE_COLOR}, and
	 * {@link #DEFAULT_NODE_LINE_COLOR}, respectively.
	 * </p>
	 * <p>
	 * The default representation for an edge is provided by a
	 * {@link y.view.PolyLineEdgeRealizer} with a standard arrow used on its
	 * target side.
	 * </p>
	 * `
	 */
	public static void configureDefaultRealizers( Graph2DView view )
	{
		NodeRealizer nr = new GenericNodeRealizer( NODE_CONFIGURATION );
		nr.setFillColor( DEFAULT_NODE_COLOR );
		nr.setLineColor( DEFAULT_NODE_LINE_COLOR );
		nr.setWidth( 60.0 );
		nr.setHeight( 30.0 );
		NodeLabel label = nr.getLabel( );
		SmartNodeLabelModel model = new SmartNodeLabelModel( );
		label.setLabelModel( model, model.getDefaultParameter( ) );
		view.getGraph2D( ).setDefaultNodeRealizer( nr );
	}

	/**
	 * Applies NodeRealizer defaults to all nodes. Properties not applied are
	 * location and size.
	 */
	public static void applyRealizerDefaults( Graph2D graph )
	{
		applyRealizerDefaults( graph, false, true );
	}

	/**
	 * Applies NodeRealizer defaults to all nodes. Properties not applied are
	 * location, and, depending on the given arguments, size and fillColor.
	 */
	public static void applyRealizerDefaults( Graph2D graph,
			boolean applyDefaultSize, boolean applyFillColor )
	{
		for ( NodeCursor nc = graph.nodes( ); nc.ok( ); nc.next( ) )
		{
			GenericNodeRealizer gnr = new GenericNodeRealizer( graph.getRealizer( nc.node( ) ) );
			gnr.setConfiguration( NODE_CONFIGURATION );
			if ( applyFillColor )
			{
				gnr.setFillColor( graph.getDefaultNodeRealizer( )
						.getFillColor( ) );
			}
			gnr.setLineColor( null );
			if ( applyDefaultSize )
			{
				gnr.setSize( graph.getDefaultNodeRealizer( ).getWidth( ),
						graph.getDefaultNodeRealizer( ).getHeight( ) );
			}
			NodeLabel label = gnr.getLabel( );
			OrientedRectangle labelBounds = label.getOrientedBox( );
			SmartNodeLabelModel model = new SmartNodeLabelModel( );
			label.setLabelModel( model,
					model.createModelParameter( labelBounds, gnr ) );
			graph.setRealizer( nc.node( ), gnr );
		}
	}

	/**
	 * Applies the given fill color to all nodes
	 */
	public static void applyFillColor( Graph2D graph, Color color )
	{
		for ( NodeCursor nc = graph.nodes( ); nc.ok( ); nc.next( ) )
		{
			Node n = nc.node( );
			graph.getRealizer( n ).setFillColor( color );
		}
	}

	/**
	 * Applies the given fill color to all nodes
	 */
	public static void applyLineColor( Graph2D graph, Color color )
	{
		for ( NodeCursor nc = graph.nodes( ); nc.ok( ); nc.next( ) )
		{
			Node n = nc.node( );
			graph.getRealizer( n ).setLineColor( color );
		}
	}

	/**
	 * Initializes to a "nice" look and feel for GUI demo applications.
	 */
	public static void initLnF( )
	{
		try
		{
			if ( !"com.sun.java.swing.plaf.motif.MotifLookAndFeel".equals( UIManager.getSystemLookAndFeelClassName( ) )
					&& !"com.sun.java.swing.plaf.gtk.GTKLookAndFeel".equals( UIManager.getSystemLookAndFeelClassName( ) )
					&& !UIManager.getSystemLookAndFeelClassName( )
							.equals( UIManager.getLookAndFeel( )
									.getClass( )
									.getName( ) )
					&& !isJRE4onWindows7( ) )
			{
				UIManager.setLookAndFeel( UIManager.getSystemLookAndFeelClassName( ) );
			}
		}
		catch ( Exception e )
		{
			e.printStackTrace( );
		}
	}

	private static boolean isJRE4onWindows7( )
	{
		// check for 'os.name == Windows 7' does not work, since JDK 1.4 uses
		// the compatibility mode
		return System.getProperty( "java.version" ).startsWith( "1.4" )
				&& System.getProperty( "os.name" ).startsWith( "Windows" )
				&& "6.1".equals( System.getProperty( "os.version" ) );
	}

}
