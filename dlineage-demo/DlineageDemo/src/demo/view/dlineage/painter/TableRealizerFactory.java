/****************************************************************************
 * This demo file is part of yFiles for Java 2.14.
 * Copyright (c) 2000-2017 by yWorks GmbH, Vor dem Kreuzberg 28,
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

package demo.view.dlineage.painter;

import java.awt.Color;
import java.util.Map;

import y.view.NodeLabel;
import y.view.YLabel;

/**
 * This is a factory for elements of Entity Relationship Diagrams (ERD).
 *
 * <p>
 * It is possible to create realizers for different kinds of ERD elements (see
 * e.g. {@link #createBigEntity()}, {@link #createSmallEntity(String)},
 * {@link #createAttribute(String)},
 * {@link #createRelation(y.view.Arrow, y.view.Arrow)}...).
 * </p>
 */
public class TableRealizerFactory
{

	/**
	 * The name of the name label configuration of a big entity.
	 * 
	 * @see #createBigEntity()
	 */
	public static final String LABEL_NAME = "demo.view.dlineage.table.label.name";

	// The two default colors for the gradient of the nodes
	private static final Color PRIMARY_COLOR = new Color( 232, 238, 247, 255 );
	private static final Color SECONDARY_COLOR = new Color( 183, 201, 227, 255 );

	/** Registers the new configurations for ERD elements */
	static
	{

		// label configurations for big entity
		registerLabelConfigurations( );

	}
	
	public static void init(){
		
	}

	/**
	 * Registers node label configurations used by the big entity node realizer.
	 * 
	 * @see #createBigEntity()
	 */
	private static void registerLabelConfigurations( )
	{
		final YLabel.Factory lf = NodeLabel.getFactory( );
		final Map lnc = lf.createDefaultConfigurationMap( );
		lnc.put( YLabel.Painter.class, new TableNameLabelPainter( ) );
		lf.addConfiguration( LABEL_NAME, lnc );
	}

}
