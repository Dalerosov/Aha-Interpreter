using System;
using System.ComponentModel;

public class Function
{
	public string name { get; set; }
	public int start_line;
    public Dictionary<string, char[]> parameters;
	public List<char[]> starting_params;
	public int called_from;
	public string where_ret;
	public char[] return_val;
	public string identifier = "";
	
	public Function(string func_name, int start_line, int called_from, Dictionary<string, char[]> parameters, string where_ret) {
		name = func_name;
		this.start_line = start_line;
		this.parameters = parameters;
		
		this.called_from = called_from;
		this.where_ret = where_ret;
		starting_params = new List<char[]>();
		identifier += func_name.Split("(")[0] + ":";
		int len = 0;
		foreach (var param in parameters) {
			char[] char_val = (char[])param.Value.Clone();
			starting_params.Add(char_val);
			identifier += new string(char_val);
			if (len < parameters.Count - 1) { 
				identifier += ",";
			}
			len++;
		}

	}

	public void add_return_val(char[] return_val)
	{
		this.return_val = return_val;
	}

	public string get_identifier() {
		return identifier;
	}
}
