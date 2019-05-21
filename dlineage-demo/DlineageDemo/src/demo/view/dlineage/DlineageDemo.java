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

package demo.view.dlineage;

import gudusoft.gsqlparser.EDbVendor;

import java.awt.BorderLayout;
import java.awt.Color;
import java.awt.Dimension;
import java.awt.EventQueue;
import java.awt.Font;
import java.awt.GraphicsEnvironment;
import java.awt.Point;
import java.awt.Rectangle;
import java.awt.Toolkit;
import java.awt.datatransfer.Clipboard;
import java.awt.datatransfer.StringSelection;
import java.awt.datatransfer.Transferable;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.ComponentAdapter;
import java.awt.event.ComponentEvent;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStream;
import java.io.PrintStream;
import java.net.URL;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.LinkedList;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.Properties;
import java.util.Set;
import java.util.logging.Handler;
import java.util.logging.Level;
import java.util.logging.LogManager;
import java.util.logging.Logger;

import javax.swing.AbstractAction;
import javax.swing.Action;
import javax.swing.Box;
import javax.swing.BoxLayout;
import javax.swing.JButton;
import javax.swing.JCheckBoxMenuItem;
import javax.swing.JDialog;
import javax.swing.JEditorPane;
import javax.swing.JFileChooser;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JMenu;
import javax.swing.JMenuBar;
import javax.swing.JMenuItem;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JRadioButtonMenuItem;
import javax.swing.JScrollPane;
import javax.swing.JSplitPane;
import javax.swing.JToolBar;
import javax.swing.JTree;
import javax.swing.ProgressMonitor;
import javax.swing.SwingUtilities;
import javax.swing.filechooser.FileFilter;
import javax.swing.text.BadLocationException;
import javax.swing.text.Element;

import jsyntaxpane.DefaultSyntaxKit;
import y.base.DataProvider;
import y.base.Edge;
import y.base.EdgeCursor;
import y.base.EdgeMap;
import y.base.Node;
import y.base.YList;
import y.layout.LayoutGraph;
import y.layout.LayoutMultiplexer;
import y.layout.LayoutOrientation;
import y.layout.Layouter;
import y.layout.PortCandidate;
import y.layout.PortConstraintConfigurator;
import y.layout.PortConstraintKeys;
import y.layout.grouping.RecursiveGroupLayouter;
import y.layout.hierarchic.IncrementalHierarchicLayouter;
import y.module.YModule;
import y.util.Comparators;
import y.util.D;
import y.util.DataProviderAdapter;
import y.util.DataProviders;
import y.util.DetailedMessagePanel;
import y.view.Bend;
import y.view.BendCursor;
import y.view.BridgeCalculator;
import y.view.DefaultGraph2DRenderer;
import y.view.EdgeRealizer;
import y.view.EditMode;
import y.view.Graph2D;
import y.view.Graph2DLayoutExecutor;
import y.view.Graph2DSelectionEvent;
import y.view.Graph2DSelectionListener;
import y.view.LineType;
import y.view.Overview;
import y.view.Selections;
import y.view.ShapeNodeRealizer;
import y.view.TooltipMode;
import y.view.YRenderingHints;
import y.view.hierarchy.DefaultHierarchyGraphFactory;
import y.view.hierarchy.DefaultNodeChangePropagator;
import y.view.hierarchy.GroupLayoutConfigurator;
import y.view.hierarchy.GroupNodeRealizer;
import y.view.hierarchy.HierarchyJTree;
import y.view.hierarchy.HierarchyManager;
import y.view.hierarchy.HierarchyTreeModel;
import y.view.hierarchy.HierarchyTreeTransferHandler;
import demo.view.DemoBase;
import demo.view.dlineage.listener.DataFlowUIAdapater;
import demo.view.dlineage.painter.CustomEdgeRealizer;
import demo.view.dlineage.painter.TableRealizerFactory;
import demos.dlineage.DataFlowAnalyzer;
import demos.dlineage.dataflow.model.RelationType;
import demos.dlineage.dataflow.model.xml.column;
import demos.dlineage.dataflow.model.xml.dataflow;
import demos.dlineage.dataflow.model.xml.table;
import demos.dlineage.util.Pair;
import demos.dlineage.util.SQLUtil;

/**
 * Demonstrates the use of <b>Nested Graph Hierarchy</b> technology and also
 * <b>Node Grouping</b>.
 * <p>
 * <b>Note:</b> <br>
 * This application demonstrates a legacy approach for interacting with nested
 * graph hierarchies and node groups. Please refer to
 * {@link demo.view.hierarchy.GroupingDemo} and
 * {@link demo.view.hierarchy.GroupNavigationDemo} for the recommended approach
 * as of yFiles for Java 2.7.
 * </p>
 * <p>
 * The main view displays a nested graph hierarchy from a specific hierarchy
 * level on downward. So-called folder nodes are used to nest graphs within
 * them, so-called group nodes are used to group a set of nodes. <br>
 * Both these types of node look similar but represent different concepts: while
 * grouped nodes still belong to the same graph as their enclosing group node,
 * the graph that is contained within a folder node is a separate entity.
 * </p>
 * <p>
 * There are several ways provided to create, modify, and navigate a graph
 * hierarchy:
 * <ul>
 * <li>By means of popup menu actions selected nodes can be grouped and also
 * nested. Reverting these operations is aOlso supported.</li>
 * <li>By Shift-dragging nodes they can be moved into and out of group
 * nodes.</li>
 * <li>Double-clicking on a folder node "drills" into the nested graph hierarchy
 * and displays only the folder node's content, i.e., effectively moves a level
 * deeper in the hierarchy. A button in the tool bar allows to move back to see
 * the folder node again (one level higher in the hierarchy).</li>
 * <li>Folder node and group node both allow switching to the other type by
 * either using popup menu actions or clicking the icon in their upper-left
 * corner.</li>
 * </ul>
 * </p>
 * <p>
 * Note that the size of group nodes is determined by the space requirements of
 * their content, i.e., their resizing behavior is restricted.
 * </p>
 * 
 * @see <a href=
 *      "http://docs.yworks.com/yfiles/doc/developers-guide/hier_mvc_model.html">Section
 *      Managing Graph Hierarchies</a> in the yFiles for Java Developer's Guide
 * @see <a href=
 *      "http://docs.yworks.com/yfiles/doc/developers-guide/hier_mvc_controller.html">Section
 *      User Interaction</a> in the yFiles for Java Developer's Guide
 */
public class DlineageDemo extends DemoBase
{

	static
	{
		Logger rootLogger = LogManager.getLogManager( ).getLogger( "" );
		rootLogger.setLevel( Level.SEVERE );
		for ( Handler h : rootLogger.getHandlers( ) )
		{
			h.setLevel( Level.SEVERE );
		}
	}
	/**
	 * The graph hierarchy manager. This is the central class for managing a
	 * hierarchy of graphs.
	 */
	protected HierarchyManager hierarchy;

	private EDbVendor vendor = EDbVendor.dbvoracle;

	private String dataflow = null;
	private JMenu vendorMenu;
	private Map<String, String> tooltipMap = new HashMap<String, String>( );
	private JCheckBoxMenuItem showLinkOnlyItem;
	private JRadioButtonMenuItem showImpactItem;
	private JRadioButtonMenuItem showRecordSetItem;
	private JRadioButtonMenuItem showDataflowItem;
	private JRadioButtonMenuItem showJoinItem;
	private JCheckBoxMenuItem simpleOutputItem;
	private boolean showLinkOnly = true;
	private RelationType showRelationType = RelationType.dataflow;
	private boolean simpleOutput = false;
	private JEditorPane sqlEditor;
	private JSplitPane tabbedPane;
	private List<Object> graphNodes;
	private boolean isScrolled;

	private Graph2D rootGraph;

	/**
	 * Instantiates this demo. Builds the GUI.
	 */
	public DlineageDemo( )
	{
		TableRealizerFactory.init( );

		rootGraph = view.getGraph2D( );

		// enable bridges for PolyLineEdgeRealizer
		BridgeCalculator bridgeCalculator = new BridgeCalculator( );
		bridgeCalculator.setCrossingMode(
				BridgeCalculator.CROSSING_MODE_HORIZONTAL_CROSSES_VERTICAL );
		( (DefaultGraph2DRenderer) view.getGraph2DRenderer( ) )
				.setBridgeCalculator( bridgeCalculator );

		view.getRenderingHints( ).put( YRenderingHints.KEY_GROUP_STATE_PAINTING,
				YRenderingHints.VALUE_GROUP_STATE_PAINTING_OFF );

		// register a hierarchy listener that will automatically adjust the
		// state of
		// the realizers that are used for the group nodes
		hierarchy.addHierarchyListener(
				new GroupNodeRealizer.StateChangeListener( ) );

		// propagates text label changes on nodes as change events
		// on the hierarchy.
		rootGraph.addGraph2DListener( new DefaultNodeChangePropagator( ) );

		Selections.SelectionStateObserver listener = new Selections.SelectionStateObserver( ) {

			protected void updateSelectionState( Graph2D graph )
			{
			}
		};
		rootGraph.addGraph2DSelectionListener( listener );
		rootGraph.addGraphListener( listener );

		rootGraph.addGraph2DSelectionListener( new Graph2DSelectionListener( ) {

			LinkedList<Graph2DSelectionEvent> events;
			protected Runnable runnable = new Runnable( ) {

				public void run( )
				{
					LinkedList<Graph2DSelectionEvent> eventsCopy = events;
					onGraph2DSelectionEvents( eventsCopy );
					events = null;
				}
			};

			protected void onGraph2DSelectionEvents(
					LinkedList<Graph2DSelectionEvent> eventsCopy )
			{
				isScrolled = false;
				sqlEditor.getHighlighter( ).removeAllHighlights( );
				Graph2DSelectionEvent event = eventsCopy.getLast( );

				if ( event.getSubject( ) instanceof Node )
				{
					Node node = (Node) event.getSubject( );

					if ( !rootGraph.isSelected( node ) )
					{
						return;
					}

					selectNodeText( node, rootGraph );

					List<Node> nodeFlow = new ArrayList<Node>( );
					nodeFlow.add( node );

					handleSelectNode( nodeFlow, node, null, true );

					nodeFlow.clear( );
					nodeFlow.add( node );

					handleSelectNode( nodeFlow, node, null, false );
				}
				if ( event.getSubject( ) instanceof Edge )
				{
					Edge edge = (Edge) event.getSubject( );

					if ( !rootGraph.isSelected( edge ) )
					{
						return;
					}

					List<Node> nodeFlow = new ArrayList<Node>( );

					handleSelectEdge( nodeFlow, edge, null, true );

					nodeFlow.clear( );

					handleSelectEdge( nodeFlow, edge, null, false );
				}
			}

			public void onGraph2DSelectionEvent( Graph2DSelectionEvent e )
			{
				if ( events == null )
				{
					events = new LinkedList<Graph2DSelectionEvent>( );
					SwingUtilities.invokeLater( runnable );
				}
				events.add( e );
			}
		} );

		// create a TreeModel, that represents the hierarchy of the nodes.
		HierarchyTreeModel htm = new HierarchyTreeModel( hierarchy );

		// use a convenience comparator that sorts the elements in the tree
		// model
		htm.setChildComparator(
				HierarchyTreeModel.createNodeStateComparator( hierarchy ) );

		// display the graph hierarchy in a special JTree using the given
		// TreeModel
		JTree tree = new HierarchyJTree( hierarchy, htm );

		// add a double click listener to the tree.
		tree.addMouseListener( new HierarchyJTreeDoubleClickListener( view ) );

		// add drag and drop functionality to HierarchyJTree. The drag and drop
		// gesture
		// will allow to reorganize the group structure using HierarchyJTree.
		tree.setDragEnabled( true );
		tree.setTransferHandler(
				new HierarchyTreeTransferHandler( hierarchy ) );

		// plug the gui elements together and add them to the pane
		JScrollPane scrollPane = new JScrollPane( tree );
		scrollPane.setPreferredSize( new Dimension( 150, 0 ) );
		JPanel leftPane = new JPanel( new BorderLayout( ) );

		view.fitContent( );

		Overview overView = new Overview( view );
		overView.setPreferredSize( new Dimension( 150, 150 ) );
		leftPane.add( overView, BorderLayout.NORTH );
		leftPane.add( scrollPane );
		leftPane.setMinimumSize( new Dimension( 150, 150 ) );

		DefaultSyntaxKit.initKit( );

		sqlEditor = new JEditorPane( );
		JScrollPane scrPane = new JScrollPane( sqlEditor );
		sqlEditor.setContentType( "text/sql" );

		tabbedPane = new JSplitPane( JSplitPane.VERTICAL_SPLIT, view, scrPane );

		final JSplitPane splitPane = new JSplitPane(
				JSplitPane.HORIZONTAL_SPLIT, leftPane, tabbedPane );

		final float[] dividerLocation = new float[]{
				0.7f
		};
		tabbedPane.addComponentListener( new ComponentAdapter( ) {

			@Override
			public void componentResized( ComponentEvent e )
			{
				tabbedPane.setDividerLocation( dividerLocation[0] );
			}
		} );

		tabbedPane.addPropertyChangeListener(
				JSplitPane.DIVIDER_LOCATION_PROPERTY,
				new PropertyChangeListener( ) {

					@Override
					public void propertyChange( PropertyChangeEvent evt )
					{
						SwingUtilities.invokeLater( new Runnable( ) {

							@Override
							public void run( )
							{
								dividerLocation[0] = (float) ( tabbedPane
										.getDividerLocation( )
										/ (float) tabbedPane
												.getBounds( ).height );
							}
						} );
					}
				} );

		contentPane.add( splitPane, BorderLayout.CENTER );

		loadInitialGraph( );
		// configure default graphics for default node realizers.
		configureDefaultGroupNodeRealizers( );
	}

	protected void configureDefaultRealizers( )
	{
		super.configureDefaultRealizers( );
	}

	protected void handleSelectEdge( List<Node> nodeFlow, Edge edge, Node node,
			boolean out )
	{
		if ( !out
				&& edge.source( ) != null
				&& edge.source( ) != node
				&& !nodeFlow.contains( edge.source( ) ) )
		{
			nodeFlow.add( edge.source( ) );
			Graph2D rootGraph = view.getGraph2D( );
			rootGraph.setSelected( edge.source( ), true );
			selectNodeText( edge.source( ), rootGraph );
			handleSelectNode( nodeFlow, edge.source( ), edge, out );
		}
		if ( out
				&& edge.target( ) != null
				&& edge.target( ) != node
				&& !nodeFlow.contains( edge.target( ) ) )
		{
			nodeFlow.add( edge.target( ) );
			Graph2D rootGraph = view.getGraph2D( );
			rootGraph.setSelected( edge.target( ), true );
			selectNodeText( edge.target( ), rootGraph );
			handleSelectNode( nodeFlow, edge.target( ), edge, out );
		}

	}

	protected void handleSelectNode( List<Node> nodeFlow, Node node, Edge edge,
			boolean out )
	{
		Graph2D rootGraph = view.getGraph2D( );

		if ( !out && node.inEdges( ).size( ) > 0 )
		{
			EdgeCursor cursor = node.inEdges( );
			Edge currentEdge = null;
			while ( ( currentEdge = (Edge) cursor.current( ) ) != null )
			{
				if ( currentEdge != edge )
				{
					rootGraph.setSelected( currentEdge, true );
					handleSelectEdge( nodeFlow, currentEdge, node, out );
				}
				cursor.next( );
			}
		}
		if ( out && node.outEdges( ).size( ) > 0 )
		{
			EdgeCursor cursor = node.outEdges( );
			Edge currentEdge = null;
			while ( ( currentEdge = (Edge) cursor.current( ) ) != null )
			{
				if ( currentEdge != edge )
				{
					rootGraph.setSelected( currentEdge, true );
					handleSelectEdge( nodeFlow, currentEdge, node, out );
				}
				cursor.next( );
			}
		}
	}

	@SuppressWarnings("unchecked")
	private void selectNodeText( Node node, Graph2D rootGraph )
	{
		if ( sqlEditor.getText( ).length( ) == 0 )
			return;
		int index = Arrays.asList( rootGraph.getNodeArray( ) ).indexOf( node );
		Object selection = graphNodes.get( index );
		if ( selection instanceof List )
		{
			List<column> columns = (List<column>) selection;
			for ( int j = 0; j < columns.size( ); j++ )
			{
				column selectedColumn = columns.get( j );

				Element root = sqlEditor.getDocument( )
						.getDefaultRootElement( );
				if ( selectedColumn.getOccurrencesNumber( ) == 0 )
					return;
				for ( int i = 0; i < selectedColumn
						.getOccurrencesNumber( ); i++ )
				{
					int startOfLineOffset = root
							.getElement(
									selectedColumn.getStartPos( i ).first - 1 )
							.getStartOffset( );

					int endOfLineOffset = root
							.getElement(
									selectedColumn.getEndPos( i ).first - 1 )
							.getStartOffset( );

					javax.swing.text.DefaultHighlighter.DefaultHighlightPainter highlightPainter = new javax.swing.text.DefaultHighlighter.DefaultHighlightPainter(
							Color.decode( "#99CCFF" ) );
					try
					{
						sqlEditor.getHighlighter( )
								.addHighlight(
										startOfLineOffset
												+ selectedColumn
														.getStartPos( i ).second
												- 1,
										endOfLineOffset
												+ selectedColumn
														.getEndPos( i ).second
												- 1,
										highlightPainter );

						if ( i == 0 && !isScrolled )
						{
							isScrolled = true;
							Rectangle viewRect = sqlEditor
									.modelToView( startOfLineOffset
											+ selectedColumn
													.getStartPos( i ).second
											- 1 );
							sqlEditor.scrollRectToVisible( viewRect );
						}
					}
					catch ( BadLocationException e )
					{
						e.printStackTrace( );
					}
				}
			}
		}
		else if ( selection instanceof column )
		{
			column selectedColumn = (column) selection;

			Element root = sqlEditor.getDocument( ).getDefaultRootElement( );
			if ( selectedColumn.getOccurrencesNumber( ) == 0 )
				return;
			for ( int i = 0; i < selectedColumn.getOccurrencesNumber( ); i++ )
			{
				int startOfLineOffset = root
						.getElement( selectedColumn.getStartPos( i ).first - 1 )
						.getStartOffset( );

				int endOfLineOffset = root
						.getElement( selectedColumn.getEndPos( i ).first - 1 )
						.getStartOffset( );

				javax.swing.text.DefaultHighlighter.DefaultHighlightPainter highlightPainter = new javax.swing.text.DefaultHighlighter.DefaultHighlightPainter(
						Color.decode( "#99CCFF" ) );
				try
				{
					sqlEditor.getHighlighter( ).addHighlight( startOfLineOffset
							+ selectedColumn.getStartPos( i ).second
							- 1,
							endOfLineOffset
									+ selectedColumn.getEndPos( i ).second
									- 1,
							highlightPainter );

					if ( !isScrolled )
					{
						isScrolled = true;
						Rectangle viewRect = sqlEditor
								.modelToView( startOfLineOffset
										+ selectedColumn.getStartPos( i ).second
										- 1 );
						sqlEditor.scrollRectToVisible( viewRect );
					}
				}
				catch ( BadLocationException e )
				{
					e.printStackTrace( );
				}
			}
		}
		else if ( selection instanceof table )
		{
			table selectedTable = (table) selection;
			Element root = sqlEditor.getDocument( ).getDefaultRootElement( );
			if ( selectedTable.getOccurrencesNumber( ) == 0 )
				return;
			for ( int i = 0; i < selectedTable.getOccurrencesNumber( ); i++ )
			{
				int startOfLineOffset = root
						.getElement( selectedTable.getStartPos( i ).first - 1 )
						.getStartOffset( );

				int endOfLineOffset = root
						.getElement( selectedTable.getEndPos( i ).first - 1 )
						.getStartOffset( );

				javax.swing.text.DefaultHighlighter.DefaultHighlightPainter highlightPainter = new javax.swing.text.DefaultHighlighter.DefaultHighlightPainter(
						Color.decode( "#99CCFF" ) );
				try
				{
					sqlEditor.getHighlighter( ).addHighlight(
							startOfLineOffset
									+ selectedTable.getStartPos( i ).second
									- 1,
							endOfLineOffset
									+ selectedTable.getEndPos( i ).second
									- 1,
							highlightPainter );
					if ( !isScrolled )
					{
						isScrolled = true;
						Rectangle viewRect = sqlEditor
								.modelToView( startOfLineOffset
										+ selectedTable.getStartPos( i ).second
										- 1 );
						sqlEditor.scrollRectToVisible( viewRect );
					}
				}
				catch ( BadLocationException e )
				{
					e.printStackTrace( );
				}
			}
		}
	}

	public JEditorPane getSqlEditor( )
	{
		return sqlEditor;
	}

	protected JMenuBar createMenuBar( )
	{
		JMenuBar menuBar = new JMenuBar( );
		JMenu menu = new JMenu( "File" );
		Action action;
		action = createLoadAction( );
		if ( action != null )
		{
			menu.add( action );
		}
		action = createLoadDirectoryAction( );
		if ( action != null )
		{
			menu.add( action );
		}
		action = createGraphXMLAction( );
		if ( action != null )
		{
			menu.add( action );
		}
		action = createRelationXMLAction( );
		if ( action != null )
		{
			menu.add( action );
		}
		menu.addSeparator( );
		menu.add( new PrintAction( ) );
		menu.addSeparator( );
		menu.add( new ExitAction( ) );
		menuBar.add( menu );

		if ( getExampleResources( ) != null
				&& getExampleResources( ).length != 0 )
		{
			createExamplesMenu( menuBar );
		}

		vendorMenu = new JMenu( "Vendor" );
		menuBar.add( vendorMenu );

		ActionListener listener = new ActionListener( ) {

			@Override
			public void actionPerformed( ActionEvent e )
			{
				if ( "BigQuery".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvbigquery;
				}
				if ( "Oracle".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvoracle;
				}
				if ( "SQL Server".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvmssql;
				}
				if ( "MySQL".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvmysql;
				}
				if ( "Sybase".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvsybase;
				}
				if ( "PostgreSQL".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvpostgresql;
				}
				if ( "Netezza".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvnetezza;
				}
				if ( "Teradata".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvteradata;
				}
				if ( "DB2".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvdb2;
				}
				if ( "Informix".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvinformix;
				}
				if ( "Greenplum".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvgreenplum;
				}
				if ( "Hive".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvhive;
				}
				if ( "Hana".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvhana;
				}
				if ( "Impala".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvimpala;
				}
				if ( "Redshift".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvredshift;
				}
				if ( "Vertica".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvvertica;
				}
				if ( "Couchbase".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvcouchbase;
				}
				if ( "Mdx".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvmdx;
				}
				if ( "OpenEdge".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvopenedge;
				}
				if ( "Snowflake".equals( e.getActionCommand( ) ) )
				{
					vendor = EDbVendor.dbvsnowflake;
				}
				for ( int i = 0; i < vendorMenu.getItemCount( ); i++ )
				{
					JMenuItem item = vendorMenu.getItem( i );
					if ( item != e.getSource( ) )
					{
						item.setSelected( false );
					}
					else
						item.setSelected( true );
				}
			}
		};

		vendorMenu.add( new JCheckBoxMenuItem( "BigQuery" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Couchbase" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "DB2" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Greenplum" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Hana" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Hive" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Impala" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Informix" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Mdx" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "MySQL" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Netezza" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "OpenEdge" ) )
				.addActionListener( listener );
		JCheckBoxMenuItem defaultItem = new JCheckBoxMenuItem( "Oracle" );
		defaultItem.setSelected( true );
		vendorMenu.add( defaultItem ).addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "PostgreSQL" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Redshift" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Snowflake" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "SQL Server" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Sybase" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Teradata" ) )
				.addActionListener( listener );
		vendorMenu.add( new JCheckBoxMenuItem( "Vertica" ) )
				.addActionListener( listener );

		menuBar.add( vendorMenu );

		final JMenu preferencesMenu = new JMenu( "Preferences" );
		menuBar.add( preferencesMenu );

		ActionListener preferencesListener = new ActionListener( ) {

			@Override
			public void actionPerformed( ActionEvent e )
			{
				if ( e.getSource( ) == showLinkOnlyItem )
				{
					if ( showLinkOnlyItem.getState( ) )
					{
						showLinkOnly = true;
					}
					else
					{
						showLinkOnly = false;
					}
				}
				if ( e.getSource( ) == showImpactItem )
				{
					showRelationType = RelationType.impact;
					showDataflowItem.setSelected( false );
					showJoinItem.setSelected( false );
					showImpactItem.setSelected( true );
					showRecordSetItem.setSelected( false );
				}
				if ( e.getSource( ) == showRecordSetItem )
				{
					showRelationType = RelationType.dataflow_recordset;
					showDataflowItem.setSelected( false );
					showJoinItem.setSelected( false );
					showImpactItem.setSelected( false );
					showRecordSetItem.setSelected( true );
				}
				if ( e.getSource( ) == showDataflowItem )
				{
					showRelationType = RelationType.dataflow;
					showImpactItem.setSelected( false );
					showJoinItem.setSelected( false );
					showDataflowItem.setSelected( true );
					showRecordSetItem.setSelected( false );
				}
				if ( e.getSource( ) == showJoinItem )
				{
					showRelationType = RelationType.join;
					showImpactItem.setSelected( false );
					showDataflowItem.setSelected( false );
					showJoinItem.setSelected( true );
					showRecordSetItem.setSelected( false );
				}
				if ( e.getSource( ) == simpleOutputItem )
				{
					if ( simpleOutputItem.getState( ) )
					{
						simpleOutput = true;
					}
					else
					{
						simpleOutput = false;
					}
				}

			}
		};

		showLinkOnlyItem = new JCheckBoxMenuItem(
				"Show Link Table Column Only" );
		showLinkOnlyItem.setSelected( true );
		preferencesMenu.add( showLinkOnlyItem )
				.addActionListener( preferencesListener );

		showDataflowItem = new JRadioButtonMenuItem( "Show Dataflow Relation" );
		showDataflowItem.setSelected( true );
		preferencesMenu.add( showDataflowItem )
				.addActionListener( preferencesListener );

		showRecordSetItem = new JRadioButtonMenuItem( "Show Dataflow Recordset Relation" );
		showRecordSetItem.setSelected( false );
		preferencesMenu.add( showRecordSetItem )
				.addActionListener( preferencesListener );
		
		showImpactItem = new JRadioButtonMenuItem( "Show Impact Relation" );
		showImpactItem.setSelected( false );
		preferencesMenu.add( showImpactItem )
				.addActionListener( preferencesListener );

		showJoinItem = new JRadioButtonMenuItem( "Show Join Relation" );
		showJoinItem.setSelected( false );
		preferencesMenu.add( showJoinItem )
				.addActionListener( preferencesListener );

		simpleOutputItem = new JCheckBoxMenuItem( "Simple Output" );
		simpleOutputItem.setSelected( false );
		preferencesMenu.add( simpleOutputItem )
				.addActionListener( preferencesListener );

		menuBar.add( preferencesMenu );

		JMenu toolsMenu = new JMenu( "Sample" );
		menuBar.add( toolsMenu );

		toolsMenu.add( new JMenuItem( new LoadInitialGraphAction( ) ) );

		JMenu logMenu = new JMenu( "Log" );
		menuBar.add( logMenu );

		logMenu.add( new JMenuItem( new ShowLogAction( ) ) );

		JMenu helpMenu = new JMenu( "Help" );
		menuBar.add( helpMenu );

		helpMenu.add( new JMenuItem( new AboutAction( ) ) );

		return menuBar;
	}

	/**
	 * Action that saves the current graph to a file in GraphML format.
	 */
	@SuppressWarnings("serial")
	public class SaveRelationXMLAction extends AbstractAction
	{

		JFileChooser chooser;

		public SaveRelationXMLAction( )
		{
			super( "Save Data Flow..." );
			chooser = null;
		}

		public void actionPerformed( ActionEvent e )
		{
			if ( chooser == null )
			{
				chooser = new JFileChooser( );
				chooser.setAcceptAllFileFilterUsed( false );
				chooser.addChoosableFileFilter( new FileFilter( ) {

					public boolean accept( File f )
					{
						return f.isDirectory( )
								|| f.getName( ).endsWith( ".xml" );
					}

					public String getDescription( )
					{
						return "Data Flow(.xml)";
					}
				} );
			}

			URL url = view.getGraph2D( ).getURL( );
			if ( url != null && "file".equals( url.getProtocol( ) ) )
			{
				chooser.setSelectedFile( new File( "dataflow.xml" ) );
			}

			if ( chooser.showSaveDialog(
					contentPane ) == JFileChooser.APPROVE_OPTION )
			{
				String name = chooser.getSelectedFile( ).toString( );
				if ( !name.endsWith( ".xml" ) )
				{
					name += ".xml";
				}
				try
				{
					SQLUtil.writeToFile( new File( name ), dataflow );
				}
				catch ( Exception e1 )
				{
					D.show( e1 );
				}
			}
		}
	}

	private Action createRelationXMLAction( )
	{
		return new SaveRelationXMLAction( );
	}

	protected Action createLoadAction( )
	{
		return new LoadAction( );
	}

	protected Action createLoadDirectoryAction( )
	{
		return new LoadDirectoryAction( );
	}

	/**
	 * Action that loads the current graph from a file in GraphML format.
	 */
	@SuppressWarnings("serial")
	public class LoadAction extends AbstractAction
	{

		JFileChooser chooser;

		public LoadAction( )
		{
			super( "Load SQL Files..." );
			chooser = null;
		}

		public void actionPerformed( ActionEvent e )
		{
			tooltipMap.clear( );

			if ( chooser == null )
			{
				chooser = new JFileChooser( );
				chooser.setAcceptAllFileFilterUsed( false );
				chooser.setMultiSelectionEnabled( true );
				chooser.addChoosableFileFilter( new FileFilter( ) {

					public boolean accept( File f )
					{
						return f.isDirectory( )
								|| f.getName( )
										.toLowerCase( )
										.endsWith( ".sql" )
								|| f.getName( )
										.toLowerCase( )
										.endsWith( ".txt" );
					}

					public String getDescription( )
					{
						return "SQL Script (.sql;.txt)";
					}
				} );
			}
			if ( chooser.showOpenDialog(
					contentPane ) == JFileChooser.APPROVE_OPTION )
			{
				if ( chooser.getSelectedFiles( ) != null )
				{
					displayDataFlow( chooser.getSelectedFiles( ) );
				}
			}

		}
	}

	@SuppressWarnings("serial")
	public class LoadDirectoryAction extends AbstractAction
	{

		JFileChooser chooser;

		public LoadDirectoryAction( )
		{
			super( "Load SQL Directory..." );
			chooser = null;
		}

		public void actionPerformed( ActionEvent e )
		{
			tooltipMap.clear( );
			File logFile = new File( ".", "dlineage.log" );
			logFile.delete( );

			view.getGraph2D( ).clear( );

			if ( chooser == null )
			{
				chooser = new JFileChooser( );
				chooser.setFileSelectionMode( JFileChooser.DIRECTORIES_ONLY );
				chooser.setAcceptAllFileFilterUsed( false );
				chooser.addChoosableFileFilter( new FileFilter( ) {

					public boolean accept( File f )
					{
						return f.isDirectory( )
								|| f.getName( )
										.toLowerCase( )
										.endsWith( ".sql" )
								|| f.getName( )
										.toLowerCase( )
										.endsWith( ".txt" );
					}

					public String getDescription( )
					{
						return "SQL Script (.sql;.txt)";
					}
				} );
			}
			if ( chooser.showOpenDialog(
					contentPane ) == JFileChooser.APPROVE_OPTION )
			{
				if ( chooser.getSelectedFile( ) != null )
				{
					displayDataFlow( chooser.getSelectedFile( ) );
				}
			}
		}
	}

	protected void configureDefaultGroupNodeRealizers( )
	{
		// Create additional configuration for default group node realizers
		DefaultHierarchyGraphFactory hgf = (DefaultHierarchyGraphFactory) hierarchy
				.getGraphFactory( );

		GroupNodeRealizer gnr = new GroupNodeRealizer( );
		// Register first, since this will also configure the node label
		gnr.setConsiderNodeLabelSize( true );

		// Nicer colors
		gnr.setFillColor( new Color( 202, 236, 255, 84 ) );
		gnr.setLineColor( Color.decode( "#666699" ) );
		gnr.setLineType( LineType.DOTTED_1 );
		gnr.getLabel( ).setBackgroundColor( Color.decode( "#99CCFF" ) );
		gnr.getLabel( ).setTextColor( Color.BLACK );
		gnr.getLabel( ).setFontSize( 15 );
		gnr.setShapeType( ShapeNodeRealizer.ROUND_RECT );

		hgf.setProxyNodeRealizerEnabled( true );

		hgf.setDefaultGroupNodeRealizer( gnr.createCopy( ) );

		// Folder nodes have a different color
		GroupNodeRealizer fnr = (GroupNodeRealizer) gnr.createCopy( );

		fnr.setFillColor( Color.decode( "#F2F0D8" ) );
		fnr.setLineColor( Color.decode( "#000000" ) );
		fnr.getLabel( ).setBackgroundColor( Color.decode( "#B7B69E" ) );

		hgf.setDefaultFolderNodeRealizer( fnr.createCopy( ) );
	}

	abstract static class RouterStrategy
	{

		abstract YModule getModule( );

		abstract void routeEdge( final Edge e );

		abstract void rerouteAdjacentEdges( final DataProvider selectedNodes,
				final Graph2D graph );

		abstract void routeEdgesToSelection( final Graph2D graph );

		abstract void route( final Graph2D graph );

		protected void routeEdge( final Edge e, final Graph2D graph )
		{
			EdgeMap spc = (EdgeMap) graph.getDataProvider(
					PortConstraintKeys.SOURCE_PORT_CONSTRAINT_KEY );
			EdgeMap tpc = (EdgeMap) graph.getDataProvider(
					PortConstraintKeys.TARGET_PORT_CONSTRAINT_KEY );

			PortConstraintConfigurator pcc = new PortConstraintConfigurator( );
			if ( spc != null && tpc != null )
			{
				spc.set( e,
						pcc.createPortConstraintFromSketch( graph,
								e,
								true,
								false ) );
				tpc.set( e,
						pcc.createPortConstraintFromSketch( graph,
								e,
								false,
								false ) );
				route( graph );
				spc.set( e, null );
				tpc.set( e, null );
			}
			else
			{
				route( graph );
			}
		}

		@SuppressWarnings({
				"rawtypes", "unchecked"
		})
		protected void routeEdgesToSelection( final Graph2D graph,
				final Object affectedEdgesKey )
		{
			final Set selectedEdges = new HashSet( );
			for ( EdgeCursor ec = graph.edges( ); ec.ok( ); ec.next( ) )
			{
				final Edge edge = ec.edge( );
				if ( graph.isSelected( edge.source( ) )
						^ graph.isSelected( edge.target( ) ) )
				{
					selectedEdges.add( edge );
					continue;
				}
				for ( BendCursor bc = graph.selectedBends( ); bc.ok( ); bc
						.next( ) )
				{
					final Bend bend = (Bend) bc.current( );
					if ( bend.getEdge( ) == edge )
					{
						selectedEdges.add( edge );
						break;
					}
				}
			}
			graph.addDataProvider( affectedEdgesKey,
					new DataProviderAdapter( ) {

						public boolean getBool( Object dataHolder )
						{
							return selectedEdges.contains( dataHolder );
						}
					} );
			route( graph );
			graph.removeDataProvider( affectedEdgesKey );
		}

		protected void routeEdge( final Edge e, final Graph2D graph,
				final Object selectedEdgesKey )
		{
			graph.addDataProvider( selectedEdgesKey,
					new DataProviderAdapter( ) {

						public boolean getBool( Object o )
						{
							return e == o;
						}
					} );
			routeEdge( e, graph );
			graph.removeDataProvider( selectedEdgesKey );
		}
	}

	protected void loadInitialGraph( )
	{
		tooltipMap.clear( );
		File logFile = new File( ".", "dlineage.log" );
		logFile.delete( );

		final File file = extractResource( "resource/example.sql" );
		displayDataFlow( new File[]{
				file
		} );
	}

	private File extractResource( String resourcePath )
	{
		try
		{
			URL resource = getResource( getClass( ), resourcePath );
			String fileName = resource.getFile( )
					.substring( resource.getFile( ).lastIndexOf( '/' ) + 1 );
			InputStream localInputStream = resource.openStream( );
			File file = new File( System.getProperty( "java.io.tmpdir" ),
					System.currentTimeMillis( ) + "/" + fileName );
			SQLUtil.writeToFile( file, localInputStream, false );
			return file;
		}
		catch ( IOException e )
		{
			D.show( e );
		}
		return null;
	}

	protected void initialize( )
	{
		// create hierarchy manager before undo manager (for view actions)
		// to ensure undo/redo works for grouped graphs
		hierarchy = new HierarchyManager( view.getGraph2D( ) );
	}

	/**
	 * Creates a toolbar for this demo.
	 */
	@SuppressWarnings("serial")
	protected JToolBar createToolBar( )
	{
		final class AnalyzeAction extends AbstractAction
		{

			AnalyzeAction( )
			{
				super( "Analyze", getIconResource( "resource/play.png" ) );
			}

			public void actionPerformed( ActionEvent e )
			{
				if ( isEnabled( ) )
				{
					setEnabled( false );
					tooltipMap.clear( );
					File logFile = new File( ".", "dlineage.log" );
					logFile.delete( );
					displayDataFlow( sqlEditor.getText( ) );
					setEnabled( true );
				}
			}
		}

		final JToolBar toolBar = super.createToolBar( );
		toolBar.addSeparator( );
		toolBar.add( createActionControl( new AnalyzeAction( ) ) );
		return toolBar;
	}

	protected TooltipMode createTooltipMode( )
	{
		TooltipMode tooltipMode = new TooltipMode( ) {

			protected String getNodeTip( Node node )
			{
				String tooltip = tooltipMap.get( node.toString( ) );
				if ( tooltip != null )
				{
					tooltip = "<html>"
							+ tooltip.replace( "&", "&amp;" )
									.replace( "<", "&lt;" )
									.replace( ">", "&gt;" )
									.replace( "\n", "<br>" )
									.replace( " ", "&nbsp;" )
									.replace( "'", "&#39;" )
									.replace( "\"", "&quot;" )

							+ "</html>";
				}
				return tooltip;
			}

			protected String getEdgeTip( Edge edge )
			{
				return null;
			}
		};
		return tooltipMode;
	}

	protected EditMode createEditMode( )
	{
		EditMode mode = super.createEditMode( );
		mode.allowBendCreation( false );
		mode.allowEdgeCreation( false );
		mode.allowLabelSelection( false );
		mode.allowMouseInput( false );
		mode.allowMoveLabels( false );
		mode.allowMovePorts( false );
		mode.allowMoveSelection( true );
		mode.allowMoving( false );
		mode.allowMovingWithPopup( false );
		mode.allowNodeCreation( false );
		mode.allowNodeEditing( false );
		mode.allowResizeNodes( false );

		return mode;
	}

	protected void registerViewModes( )
	{
		view.addViewMode( createEditMode( ) );
		view.addViewMode( createTooltipMode( ) );
	}

	/**
	 * Launches this demo.
	 */
	public static void main( String[] args )
	{
		EventQueue.invokeLater( new Runnable( ) {

			public void run( )
			{
				Locale.setDefault( Locale.ENGLISH );
				initLnF( );
				( new DlineageDemo( ) ).start( "Data Flow Demo" );
			}
		} );
	}

	@SuppressWarnings("serial")
	class LoadInitialGraphAction extends AbstractAction
	{

		LoadInitialGraphAction( )
		{
			super( "Load Sample SQL" );
		}

		public void actionPerformed( ActionEvent ae )
		{
			view.getGraph2D( ).clear( );
			final EDbVendor originalVerdor = vendor;
			vendor = EDbVendor.dbvoracle;
			loadInitialGraph( );
			SwingUtilities.invokeLater( new Runnable( ) {

				public void run( )
				{
					vendor = originalVerdor;
				}
			} );
		}
	}

	@SuppressWarnings("serial")
	class ShowLogAction extends AbstractAction
	{

		ShowLogAction( )
		{
			super( "Show Log..." );
		}

		public void actionPerformed( ActionEvent ae )
		{
			run( null );
		}

		private void run( String information )
		{
			File logFile = new File( ".", "dlineage.log" );
			if ( logFile.exists( ) && logFile.length( ) > 0 )
			{
				String logContent = SQLUtil.getFileContent( logFile );
				String[] splits = logContent.split( "\n" );
				List<String> logs = new ArrayList<String>( );
				StringBuffer buffer = new StringBuffer( );
				for ( int i = 0; i < splits.length; i++ )
				{
					String split = splits[i].trim( );
					if ( !logs.contains( split ) )
					{
						logs.add( split );
						buffer.append( split ).append( "\n" );
					}
				}

				final DetailedMessagePanel panel = new DetailedMessagePanel(
						null,
						information != null ? information
								: "Please see logs for details:",
						buffer.toString( ),
						false );
				panel.setFont( new Font( "Courier New", Font.PLAIN, 12 ) );
				panel.setDetailsShowing( true );
				SwingUtilities.invokeLater( new Runnable( ) {

					public void run( )
					{
						panel.show( null, 1, "Log" );
					}
				} );
			}
			else
			{
				SwingUtilities.invokeLater( new Runnable( ) {

					public void run( )
					{
						DetailedMessagePanel.show( null,
								"Log",
								1,
								null,
								"Log not found." );
					}
				} );
			}
		}
	}

	class AboutDialog extends JDialog
	{

		private static final long serialVersionUID = -6346454462812009889L;

		public AboutDialog( )
		{
			String buildVersion = "1.0.0";
			String gspVersion = "1.8.8.4";
			String buildDate = "2017.10.05";

			try
			{
				Properties pps = new Properties( );
				pps.load( this.getClass( ).getResourceAsStream(
						"/demo/view/dlineage/resource/version.properties" ) );
				buildVersion = pps.getProperty( "build.version" );
				gspVersion = pps.getProperty( "gsp.version" );
				buildDate = pps.getProperty( "build.date" );
			}
			catch ( IOException e )
			{
				e.printStackTrace( );
			}

			setTitle( "About Data Flow Demo" );
			setIconImage( getFrameIcon( ) );
			setLayout( new BoxLayout( getContentPane( ), BoxLayout.Y_AXIS ) );

			add( Box.createRigidArea( new Dimension( 0, 20 ) ) );

			JLabel demoVersionLabel = new JLabel(
					"Build Version: " + buildVersion );
			demoVersionLabel.setAlignmentX( 0.5f );
			add( demoVersionLabel );

			add( Box.createRigidArea( new Dimension( 0, 10 ) ) );

			JLabel buildVersionLabel = new JLabel( "Build Date: " + buildDate );
			buildVersionLabel.setAlignmentX( 0.5f );
			add( buildVersionLabel );

			add( Box.createRigidArea( new Dimension( 0, 10 ) ) );

			JLabel gspVersionLabel = new JLabel( "GSP Version: " + gspVersion );
			gspVersionLabel.setAlignmentX( 0.5f );
			add( gspVersionLabel );

			add( Box.createRigidArea( new Dimension( 0, 30 ) ) );

			JButton close = new JButton( "Close" );
			close.addActionListener( new ActionListener( ) {

				public void actionPerformed( ActionEvent event )
				{
					dispose( );
				}
			} );

			close.setAlignmentX( 0.5f );
			add( close );
			setModalityType( ModalityType.APPLICATION_MODAL );
			setDefaultCloseOperation( DISPOSE_ON_CLOSE );

			int DIALOG_WIDTH = 300;
			int DIALOG_HEIGHT = 300;

			setSize( DIALOG_WIDTH, DIALOG_HEIGHT );

			Point point = GraphicsEnvironment.getLocalGraphicsEnvironment( )
					.getCenterPoint( );
			this.setBounds( point.x - DIALOG_WIDTH / 2,
					point.y - DIALOG_HEIGHT / 2,
					DIALOG_WIDTH,
					DIALOG_HEIGHT - 100 );
		}
	}

	@SuppressWarnings("serial")
	class AboutAction extends AbstractAction
	{

		AboutAction( )
		{
			super( "About..." );
		}

		public void actionPerformed( ActionEvent ae )
		{
			run( );
		}

		private void run( )
		{
			new AboutDialog( ).setVisible( true );
		}
	}

	/**
	 * Layouts the nodes (rows) within the group nodes (tables).
	 */
	static class RowLayouter implements Layouter
	{

		private static final double DISTANCE = 0.0;

		public boolean canLayout( LayoutGraph graph )
		{
			return graph.edgeCount( ) == 0;
		}

		@SuppressWarnings({
				"rawtypes", "unchecked"
		})
		public void doLayout( final LayoutGraph graph )
		{
			Node[] rows = graph.getNodeArray( );
			Arrays.sort( rows, new Comparator( ) {

				public int compare( Object o1, Object o2 )
				{
					return Comparators.compare( graph.getCenterY( (Node) o1 ),
							graph.getCenterY( (Node) o2 ) );
				}
			} );

			double currentY = 0.0;
			for ( int i = 0; i < rows.length; i++ )
			{
				// set layout of row
				graph.setLocation( rows[i], 0.0, currentY );
				currentY += graph.getHeight( rows[i] ) + DISTANCE;
			}
		}
	}

	private synchronized void displayDataFlow( final Object selections )
	{
		if ( !( selections instanceof String ) )
		{
			sqlEditor.setText( "" );
			if ( selections instanceof File )
			{
				if ( ( (File) selections ).isFile( ) )
				{
					String text = SQLUtil.getFileContent( (File) selections );
					Clipboard clipboard = Toolkit.getDefaultToolkit( )
							.getSystemClipboard( );
					Transferable trans = new StringSelection( text );
					clipboard.setContents( trans, null );
					sqlEditor.paste( );
				}
			}
			else if ( selections instanceof File[]
					&& ( (File[]) selections ).length == 1 )
			{
				if ( ( ( (File[]) selections )[0] ).isFile( ) )
				{
					String text = SQLUtil
							.getFileContent( ( (File[]) selections )[0] );
					Clipboard clipboard = Toolkit.getDefaultToolkit( )
							.getSystemClipboard( );
					Transferable trans = new StringSelection( text );
					clipboard.setContents( trans, null );
					sqlEditor.paste( );
				}
			}
		}

		new Thread( ) {

			public void run( )
			{
				final File outputDir = new File( "./output" );

				if ( outputDir.exists( ) )
				{
					SQLUtil.deltree( outputDir );
				}
				outputDir.mkdirs( );

				final ProgressMonitor monitor = new ProgressMonitor(
						contentPane.getParent( ),
						"Analyzing Progress",
						"",
						0,
						1000000 );

				final DataFlowUIAdapater adapter = new DataFlowUIAdapater(
						monitor, new File( outputDir, "dataflow.log" ) );

				view.getGraph2D( ).clear( );
				view.getGraph2D( ).updateViews( );

				DataFlowAnalyzer dlineage;
				if ( selections instanceof File )
				{
					dlineage = new DataFlowAnalyzer( (File) selections,
							vendor,
							(simpleOutput && showRelationType == RelationType.dataflow) );
				}
				else if ( selections instanceof File[] )
				{
					dlineage = new DataFlowAnalyzer( (File[]) selections,
							vendor,
							(simpleOutput && showRelationType == RelationType.dataflow) );
				}
				else if ( selections instanceof String )
				{
					dlineage = new DataFlowAnalyzer( (String) selections,
							vendor,
							(simpleOutput && showRelationType == RelationType.dataflow) );
				}
				else
				{
					return;
				}

				dlineage.setShowJoin( showRelationType == RelationType.join );
				dlineage.setHandleListener( adapter );

				final StringBuffer errorMessage = new StringBuffer( );

				dataflow = dlineage.generateDataFlow( errorMessage );

				if ( adapter.isCanceled( ) )
				{
					return;
				}

				try
				{
					SQLUtil.writeToFile( new File( outputDir, "dataflow.xml" ),
							dataflow );
				}
				catch ( IOException e )
				{
					e.printStackTrace( );
				}

				if ( errorMessage.length( ) > 0 )
				{
					File logFile = new File( ".", "dlineage.log" )
							.getAbsoluteFile( );
					logFile.delete( );

					PrintStream pw = null;
					try
					{
						pw = new PrintStream( logFile );
						pw.print( errorMessage );
					}
					catch ( FileNotFoundException e1 )
					{
						e1.printStackTrace( );
					}
					if ( pw != null )
					{
						pw.close( );
					}
					System.err.println( errorMessage );
				}

				final String message = errorMessage.toString( )
						.replaceAll( "Orphan column.+", "" )
						.trim( );

				SQLUtil.resetVirtualTableNames( );

				adapter.startConvertXMLToModel( );

				dataflow dataflow = dlineage.getDataFlow( );

				adapter.endConvertXMLToModel( );

				if ( dataflow == null )
					return;

				adapter.startConvertModelToGraphXML( );

				DataFlowGraph dataFlowGraph = new DataFlowGraph( );
				final Pair<String, Boolean> graphContent = dataFlowGraph
						.generateDataFlowGraph( dataflow,
								showLinkOnly,
								showRelationType,
								tooltipMap );

				graphNodes = dataFlowGraph.getNodes( );

				final File file = new File(
						System.getProperty( "java.io.tmpdir" ),
						System.currentTimeMillis( ) + "/graph.graphml" );

				try
				{
					if ( graphContent != null && graphContent.first != null )
					{
						String content = graphContent.first
								.replace( "\r\n", "\n" ).replace( "\n", " " );
						SQLUtil.writeToFile( file, content );
						SQLUtil.writeToFile(
								new File( outputDir, "dataflow.graphml" ),
								content );
					}
					else
					{
						String content = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><graphml xmlns=\"http://graphml.graphdrawing.org/xmlns\" xmlns:java=\"http://www.yworks.com/xml/yfiles-common/1.0/java\" xmlns:sys=\"http://www.yworks.com/xml/yfiles-common/markup/primitives/2.0\" xmlns:x=\"http://www.yworks.com/xml/yfiles-common/markup/2.0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:y=\"http://www.yworks.com/xml/graphml\" xsi:schemaLocation=\"http://graphml.graphdrawing.org/xmlns http://www.yworks.com/xml/schema/graphml/1.1/ygraphml.xsd\"></graphml>";
						SQLUtil.writeToFile( file, content );
						SQLUtil.writeToFile(
								new File( outputDir, "dataflow.graphml" ),
								content );
					}
				}
				catch ( final Exception e1 )
				{
					monitor.setProgress( monitor.getMaximum( ) );
					SwingUtilities.invokeLater( new Runnable( ) {

						public void run( )
						{
							D.show( e1 );
						}
					} );
					return;
				}
				finally
				{
					adapter.endConvertModelToGraphXML( );
				}

				adapter.startShowGraph( );

				SwingUtilities.invokeLater( new Runnable( ) {

					public void run( )
					{

						try
						{
							URL resource = file.toURI( ).toURL( );

							if ( resource != null )
							{
								adapter.startLoadGraphXML( );
								loadGraph( resource );
								adapter.endLoadGraphXML( );

								SwingUtilities.invokeLater( new Runnable( ) {

									public void run( )
									{
										new Thread( ) {

											private void updateEdgeRealizers( )
											{
												final Graph2D graph2D = view
														.getGraph2D( );

												for ( EdgeCursor ec = graph2D
														.edges( ); ec.ok( ); ec
																.next( ) )
												{
													Edge edge = ec.edge( );
													EdgeRealizer oldRealizer = graph2D
															.getRealizer(
																	edge );
													EdgeRealizer newRealizer = new CustomEdgeRealizer(
															oldRealizer );
													for ( BendCursor bc = oldRealizer
															.bends( ); bc
																	.ok( ); bc
																			.next( ) )
													{
														Bend bend = bc.bend( );
														newRealizer.addPoint(
																bend.getX( ),
																bend.getY( ) );
													}
													graph2D.setRealizer( edge,
															newRealizer );
												}
												view.updateView( );
											}

											public void run( )
											{
												if ( adapter != null
														&& adapter
																.isCanceled( ) )
												{
													return;
												}

												try
												{
													adapter.startLayoutGraph( );
													YList candidates = new YList( );
													candidates
															.add( PortCandidate
																	.createCandidate(
																			PortCandidate.WEST ) );
													candidates
															.add( PortCandidate
																	.createCandidate(
																			PortCandidate.EAST ) );
													view.getGraph2D( )
															.addDataProvider(
																	PortCandidate.SOURCE_PCLIST_DPKEY,
																	DataProviders
																			.createConstantDataProvider(
																					candidates ) );
													view.getGraph2D( )
															.addDataProvider(
																	PortCandidate.TARGET_PCLIST_DPKEY,
																	DataProviders
																			.createConstantDataProvider(
																					candidates ) );

													final RowLayouter rowLayouter = new RowLayouter( );
													final IncrementalHierarchicLayouter ihl = new IncrementalHierarchicLayouter( );
													ihl.setLayoutOrientation(
															LayoutOrientation.LEFT_TO_RIGHT );
													ihl.setOrthogonallyRouted(
															true );
													ihl.setMinimumLayerDistance(
															50 );

													view.getGraph2D( )
															.addDataProvider(
																	RecursiveGroupLayouter.GROUP_NODE_LAYOUTER_DPKEY,
																	new DataProviderAdapter( ) {

																		public Object get(
																				Object dataHolder )
																		{
																			return rowLayouter;
																		}
																	} );

													// prepare grouping
													// information
													GroupLayoutConfigurator glc = new GroupLayoutConfigurator(
															view.getGraph2D( ) );
													try
													{
														glc.prepareAll( );

														// do layout
														RecursiveGroupLayouter rgl = new RecursiveGroupLayouter(
																ihl );
														rgl.setAutoAssignPortCandidatesEnabled(
																true );
														rgl.setConsiderSketchEnabled(
																true );
														new Graph2DLayoutExecutor( )
																.doLayout( view,
																		rgl );
													}
													finally
													{
														// dispose
														glc.restoreAll( );
														view.getGraph2D( )
																.removeDataProvider(
																		PortCandidate.SOURCE_PCLIST_DPKEY );
														view.getGraph2D( )
																.removeDataProvider(
																		PortCandidate.TARGET_PCLIST_DPKEY );
														view.getGraph2D( )
																.removeDataProvider(
																		LayoutMultiplexer.LAYOUTER_DPKEY );
													}
													updateEdgeRealizers( );

													view.updateView( );
													view.fitContent( );

													adapter.endLayoutGraph( );
													adapter.endShowGraph( );
													monitor.setProgress( monitor
															.getMaximum( ) );
												}
												catch ( Exception e )
												{
													monitor.setProgress( monitor
															.getMaximum( ) );
													D.show( e );
												}
											}
										}.start( );
									}
								} );
							}

							if ( message.length( ) > 0 )
							{
								JOptionPane.showMessageDialog( new JFrame( ),
										"SQL syntax error is detected, please check dlineage.log for more information!",
										"Dialog",
										JOptionPane.ERROR_MESSAGE );
							}
						}
						catch ( Exception e1 )
						{
							monitor.setProgress( monitor.getMaximum( ) );
							D.show( e1 );
						}
					}
				} );

			}
		}.start( );
	}
}
