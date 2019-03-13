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
import java.awt.Graphics2D;
import java.awt.Stroke;
import java.awt.geom.Line2D;
import java.util.Map;

import y.geom.OrientedRectangle;
import y.view.NodeLabel;
import y.view.YLabel;

/**
 * This painter draws the name label of a detailed entity of an entity
 * relationship diagram (ERD).
 *
 * The painter does not draw an opaque background as the
 * <code>ErdNodePainter</code> already paints all background colors.
 *
 * @see ErdNodePainter
 */
public class TableNameLabelPainter implements YLabel.Painter
{

	private final YLabel.Painter painter;

	/** Creates a new <code>ErdNameLabelPainter</code> */
	public TableNameLabelPainter( )
	{
		this( defaultPainter( ) );
	}

	/**
	 * Creates a new <code>ErdNameLabelPainter</code> that uses the given label
	 * painter.
	 * 
	 * @param painter
	 *            the label painter
	 */
	public TableNameLabelPainter( final YLabel.Painter painter )
	{
		this.painter = painter;
	}

	/**
	 * Paints the label.
	 * 
	 * @param label
	 *            the label context
	 * @param gfx
	 *            the grapics object
	 */
	public void paint( final YLabel label, final Graphics2D gfx )
	{
		if ( painter != null )
		{
			painter.paint( label, gfx );
		}
	}

	/**
	 * Paints the label content to the specified location. In this case it is a
	 * string with contains the name of the entity.
	 * 
	 * @param label
	 *            the label context
	 * @param gfx
	 *            the graphics object
	 * @param x
	 *            the x-coordinate of the label
	 * @param y
	 *            the y-coordinate of the label
	 * @param width
	 *            the width of the label
	 * @param height
	 *            the height of the label
	 */
	public void paintContent( final YLabel label, final Graphics2D gfx,
			final double x, final double y, final double width,
			final double height )
	{
		if ( painter != null )
		{
			painter.paintContent( label, gfx, x, y, width, height );
		}
	}

	/**
	 * Paints the background of the label.
	 * 
	 * @param label
	 *            the label context
	 * @param gfx
	 *            the graphics object
	 * @param x
	 *            the x-coordinate of the label
	 * @param y
	 *            the y-coordinate of the label
	 * @param width
	 *            the width of the label
	 * @param height
	 *            the height of the label
	 */
	public void paintBox( final YLabel label, final Graphics2D gfx,
			final double x, final double y, final double width,
			final double height )
	{
		// paint the box only if the *main* painter is not null
		if ( painter != null )
		{
			painter.paintBox( label, gfx, x, y, width, height + 2 );

			// Draw line with line type of context
			Color lc = label.getLineColor( );
			if ( lc == null )
			{
				lc = label.getTextColor( );
			}
			if ( lc != null )
			{
				Stroke oldStroke = gfx.getStroke( );
				gfx.setColor( lc );

				final Line2D.Double line = new Line2D.Double( );
				line.setLine( x, y + height + 2, x + width, y + height + 2 );
				gfx.draw( line );

				gfx.setStroke( oldStroke );
			}
		}
	}

	/**
	 * Returns the text box of a given label
	 * 
	 * @param label
	 *            the label context
	 * @return a rectangle with the size and position of the text box
	 */
	public OrientedRectangle getTextBox( final YLabel label )
	{
		if ( painter != null )
		{
			return painter.getTextBox( label );
		}
		else
		{
			return null;
		}
	}

	/**
	 * Returns the icon box of a given label
	 * 
	 * @param label
	 *            the label context
	 * @return a rectangle with the size and position of the icon box
	 */
	public OrientedRectangle getIconBox( final YLabel label )
	{
		if ( painter != null )
		{
			return painter.getIconBox( label );
		}
		else
		{
			return null;
		}
	}

	/**
	 * Returns the default painter if it exists.
	 * 
	 * @return the default painter or null if it does not exist
	 */
	static YLabel.Painter defaultPainter( )
	{
		final YLabel.Factory factory = NodeLabel.getFactory( );
		final Map c = factory.createDefaultConfigurationMap( );
		final Object painter = c.get( YLabel.Painter.class );
		if ( painter instanceof YLabel.Painter )
		{
			return (YLabel.Painter) painter;
		}
		else
		{
			return null;
		}
	}
}
