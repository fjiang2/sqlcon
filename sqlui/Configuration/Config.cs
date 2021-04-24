using System;
using System.Collections.Generic;
using Sys;

namespace sqlcon
{
	static partial class Config
	{
	
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
		
		
	
	}
}