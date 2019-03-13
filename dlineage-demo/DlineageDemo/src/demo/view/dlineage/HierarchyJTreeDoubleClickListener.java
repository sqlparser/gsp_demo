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

import javax.swing.tree.*;
import javax.swing.*;
import java.awt.event.*;
import y.view.*;
import y.base.*;

/**
 * A MouseListener that listens for double click events on a JTree.
 * The node item that was clicked will be focused in an 
 * associated Graph2DView.
 */ 
public class HierarchyJTreeDoubleClickListener extends MouseAdapter
{
  Graph2DView view;
  
  public HierarchyJTreeDoubleClickListener(Graph2DView view)
  {
    this.view = view;
  }
  
  public void mouseClicked(MouseEvent e)
  {
    JTree tree =(JTree)e.getSource();
    
    if(e.getClickCount() == 2)
    {
      //D.bug("right mouse pressed");
      
      int y = e.getY();
      int x = e.getX();
      TreePath path = tree.getPathForLocation(x,y);
      if(path != null)
      {
        Object last =  path.getLastPathComponent();
        Graph2D focusedGraph = null;
        Node v = null;
        
        if(last instanceof Node)
        {
          v = (Node)last;
          focusedGraph = (Graph2D)v.getGraph();
        }
        else if(last instanceof Graph2D) //root
        {
          focusedGraph = (Graph2D)last;
        }
        
        if(focusedGraph != null)
        {
          view.setGraph2D(focusedGraph);
          if(v != null)
          {
            view.setCenter(focusedGraph.getCenterX(v),focusedGraph.getCenterY(v));
          }
          view.updateView();
        }
      }
    }
  }
}
