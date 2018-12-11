
package demos.dlineage.dataflow.model.xml;

import java.util.List;

import org.simpleframework.xml.ElementList;
import org.simpleframework.xml.Root;

@Root(name = "dlineage")
public class dataflow
{

	@ElementList(entry = "relation", inline = true, required = false)
	private List<relation> relations;

	@ElementList(entry = "table", inline = true, required = false)
	private List<table> tables;

	@ElementList(entry = "view", inline = true, required = false)
	private List<table> views;

	@ElementList(entry = "resultset", inline = true, required = false)
	private List<table> resultsets;

	public List<relation> getRelations( )
	{
		return relations;
	}

	public void setRelations( List<relation> relations )
	{
		this.relations = relations;
	}

	public List<table> getTables( )
	{
		return tables;
	}

	public void setTables( List<table> tables )
	{
		this.tables = tables;
	}

	public List<table> getViews( )
	{
		return views;
	}

	public void setViews( List<table> views )
	{
		this.views = views;
	}

	public List<table> getResultsets( )
	{
		return resultsets;
	}

	public void setResultsets( List<table> resultsets )
	{
		this.resultsets = resultsets;
	}

}