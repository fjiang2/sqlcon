using System;
using System.Collections.Generic;
using Sys;

namespace sqlcon
{
	public static partial class Config
	{
		//editor
		public static string editor => cfg.GetValue<string>(ConfigKey._EDITOR, ConfigDefaultValue.__EDITOR);
		
		//directory
		public static string directory => cfg.GetValue<string>(ConfigKey._DIRECTORY, ConfigDefaultValue.__DIRECTORY);
		
		//output
		public static string output => cfg.GetValue<string>(ConfigKey._OUTPUT, ConfigDefaultValue.__OUTPUT);
		
		//log
		public static string log => cfg.GetValue<string>(ConfigKey._LOG, ConfigDefaultValue.__LOG);
		
		//xmldb
		public static string xmldb => cfg.GetValue<string>(ConfigKey._XMLDB, ConfigDefaultValue.__XMLDB);
		
		public static class options
		{
			public static class Comparison
			{
				//options.Comparison.IgnoreCase
				public static bool IgnoreCase => cfg.GetValue<bool>(ConfigKey._OPTIONS_COMPARISON_IGNORECASE, ConfigDefaultValue.__OPTIONS_COMPARISON_IGNORECASE);
			}
		}
		
		public static class console
		{
			public static class table
			{
				public static class grid
				{
					//console.table.grid.MaxColumnWidth
					public static int MaxColumnWidth => cfg.GetValue<int>(ConfigKey._CONSOLE_TABLE_GRID_MAXCOLUMNWIDTH, ConfigDefaultValue.__CONSOLE_TABLE_GRID_MAXCOLUMNWIDTH);
					
					//console.table.grid.MaxRows
					public static int MaxRows => cfg.GetValue<int>(ConfigKey._CONSOLE_TABLE_GRID_MAXROWS, ConfigDefaultValue.__CONSOLE_TABLE_GRID_MAXROWS);
				}
			}
		}
		
		public static class gui
		{
			public static class table
			{
				public static class editor
				{
					//gui.table.editor.AlternatingRowBackground
					public static string AlternatingRowBackground => cfg.GetValue<string>(ConfigKey._GUI_TABLE_EDITOR_ALTERNATINGROWBACKGROUND, ConfigDefaultValue.__GUI_TABLE_EDITOR_ALTERNATINGROWBACKGROUND);
					
					//gui.table.editor.Foreground
					public static string Foreground => cfg.GetValue<string>(ConfigKey._GUI_TABLE_EDITOR_FOREGROUND, ConfigDefaultValue.__GUI_TABLE_EDITOR_FOREGROUND);
					
					//gui.table.editor.Background
					public static string Background => cfg.GetValue<string>(ConfigKey._GUI_TABLE_EDITOR_BACKGROUND, ConfigDefaultValue.__GUI_TABLE_EDITOR_BACKGROUND);
					
					//gui.table.editor.RowBackground
					public static string RowBackground => cfg.GetValue<string>(ConfigKey._GUI_TABLE_EDITOR_ROWBACKGROUND, ConfigDefaultValue.__GUI_TABLE_EDITOR_ROWBACKGROUND);
				}
			}
			
			public static class sql
			{
				public static class editor
				{
					//gui.sql.editor.Foreground
					public static string Foreground => cfg.GetValue<string>(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, ConfigDefaultValue.__GUI_SQL_EDITOR_FOREGROUND);
					
					//gui.sql.editor.Background
					public static string Background => cfg.GetValue<string>(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, ConfigDefaultValue.__GUI_SQL_EDITOR_BACKGROUND);
				}
				
				public static class result
				{
					public static class table
					{
						//gui.sql.result.table.Foreground
						public static string Foreground => cfg.GetValue<string>(ConfigKey._GUI_SQL_RESULT_TABLE_FOREGROUND, ConfigDefaultValue.__GUI_SQL_RESULT_TABLE_FOREGROUND);
						
						//gui.sql.result.table.Background
						public static string Background => cfg.GetValue<string>(ConfigKey._GUI_SQL_RESULT_TABLE_BACKGROUND, ConfigDefaultValue.__GUI_SQL_RESULT_TABLE_BACKGROUND);
						
						//gui.sql.result.table.AlternatingRowBackground
						public static string AlternatingRowBackground => cfg.GetValue<string>(ConfigKey._GUI_SQL_RESULT_TABLE_ALTERNATINGROWBACKGROUND, ConfigDefaultValue.__GUI_SQL_RESULT_TABLE_ALTERNATINGROWBACKGROUND);
						
						//gui.sql.result.table.RowBackground
						public static string RowBackground => cfg.GetValue<string>(ConfigKey._GUI_SQL_RESULT_TABLE_ROWBACKGROUND, ConfigDefaultValue.__GUI_SQL_RESULT_TABLE_ROWBACKGROUND);
					}
					
					public static class message
					{
						//gui.sql.result.message.Foreground
						public static string Foreground => cfg.GetValue<string>(ConfigKey._GUI_SQL_RESULT_MESSAGE_FOREGROUND, ConfigDefaultValue.__GUI_SQL_RESULT_MESSAGE_FOREGROUND);
						
						//gui.sql.result.message.Background
						public static string Background => cfg.GetValue<string>(ConfigKey._GUI_SQL_RESULT_MESSAGE_BACKGROUND, ConfigDefaultValue.__GUI_SQL_RESULT_MESSAGE_BACKGROUND);
					}
				}
			}
		}
		
		public static class generator
		{
			public static class dpo
			{
				//generator.dpo.path
				public static string path => cfg.GetValue<string>(ConfigKey._GENERATOR_DPO_PATH, ConfigDefaultValue.__GENERATOR_DPO_PATH);
				
				//generator.dpo.ns
				public static string ns => cfg.GetValue<string>(ConfigKey._GENERATOR_DPO_NS, ConfigDefaultValue.__GENERATOR_DPO_NS);
				
				//generator.dpo.suffix
				public static string suffix => cfg.GetValue<string>(ConfigKey._GENERATOR_DPO_SUFFIX, ConfigDefaultValue.__GENERATOR_DPO_SUFFIX);
				
				//generator.dpo.level
				public static int level => cfg.GetValue<int>(ConfigKey._GENERATOR_DPO_LEVEL, ConfigDefaultValue.__GENERATOR_DPO_LEVEL);
				
				//generator.dpo.HasProvider
				public static bool HasProvider => cfg.GetValue<bool>(ConfigKey._GENERATOR_DPO_HASPROVIDER, ConfigDefaultValue.__GENERATOR_DPO_HASPROVIDER);
				
				//generator.dpo.hasTableAttr
				public static bool hasTableAttr => cfg.GetValue<bool>(ConfigKey._GENERATOR_DPO_HASTABLEATTR, ConfigDefaultValue.__GENERATOR_DPO_HASTABLEATTR);
				
				//generator.dpo.hasColumnAttr
				public static bool hasColumnAttr => cfg.GetValue<bool>(ConfigKey._GENERATOR_DPO_HASCOLUMNATTR, ConfigDefaultValue.__GENERATOR_DPO_HASCOLUMNATTR);
				
				//generator.dpo.IsPack
				public static bool IsPack => cfg.GetValue<bool>(ConfigKey._GENERATOR_DPO_ISPACK, ConfigDefaultValue.__GENERATOR_DPO_ISPACK);
			}
			
			public static class dc
			{
				//generator.dc.path
				public static string path => cfg.GetValue<string>(ConfigKey._GENERATOR_DC_PATH, ConfigDefaultValue.__GENERATOR_DC_PATH);
				
				//generator.dc.ns
				public static string ns => cfg.GetValue<string>(ConfigKey._GENERATOR_DC_NS, ConfigDefaultValue.__GENERATOR_DC_NS);
			}
			
			public static class l2s
			{
				//generator.l2s.path
				public static string path => cfg.GetValue<string>(ConfigKey._GENERATOR_L2S_PATH, ConfigDefaultValue.__GENERATOR_L2S_PATH);
				
				//generator.l2s.ns
				public static string ns => cfg.GetValue<string>(ConfigKey._GENERATOR_L2S_NS, ConfigDefaultValue.__GENERATOR_L2S_NS);
			}
			
			public static class de
			{
				//generator.de.path
				public static string path => cfg.GetValue<string>(ConfigKey._GENERATOR_DE_PATH, ConfigDefaultValue.__GENERATOR_DE_PATH);
				
				//generator.de.ns
				public static string ns => cfg.GetValue<string>(ConfigKey._GENERATOR_DE_NS, ConfigDefaultValue.__GENERATOR_DE_NS);
			}
			
			public static class ds
			{
				//generator.ds.path
				public static string path => cfg.GetValue<string>(ConfigKey._GENERATOR_DS_PATH, ConfigDefaultValue.__GENERATOR_DS_PATH);
			}
			
			public static class csv
			{
				//generator.csv.path
				public static string path => cfg.GetValue<string>(ConfigKey._GENERATOR_CSV_PATH, ConfigDefaultValue.__GENERATOR_CSV_PATH);
			}
		}
		
		public static class servers
		{
		}
		
		public static class limit
		{
			//limit.top
			public static int top => cfg.GetValue<int>(ConfigKey._LIMIT_TOP, ConfigDefaultValue.__LIMIT_TOP);
			
			//limit.export_max_count
			public static int export_max_count => cfg.GetValue<int>(ConfigKey._LIMIT_EXPORT_MAX_COUNT, ConfigDefaultValue.__LIMIT_EXPORT_MAX_COUNT);
		}
	}
}