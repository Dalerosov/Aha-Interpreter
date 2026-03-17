
using System.Text.RegularExpressions;


class Aha
{
    // ILL USE THIS FOR NOW, DELETE LATER
    private static string filePath = "C:/aha/prog.aha";
    private static List<string> code;
    private static int line_numb = 0;

    static Dictionary<string, char[]> vars = new Dictionary<string, char[]>();
    static List<Function> functions = new List<Function>();
    static Dictionary<string, Function> cached_functions = new Dictionary<string, Function>();
    static Dictionary<string, (int, int)> sectors = new Dictionary<string, (int, int)>();
    
    static string last_added_sector = "";
    static int steps = 0;

    private static void OnApplicationExit(object sender, ConsoleCancelEventArgs e) {
        Thread.Sleep(1000);
        Console.WriteLine("closing");
        foreach (var func in cached_functions.Values) {
            Console.WriteLine(func.identifier + ": " + new string(func.return_val));
        }
    }

    static void Main(string[] args)
    {
        //Console.CancelKeyPress += new ConsoleCancelEventHandler(OnApplicationExit);
        interpeter_settings_base_setup();
        code = File.ReadAllLines(filePath).ToList<string>();
        code.Insert(0, "SECSTART BASE");
        code.Add("SECEND BASE");
        include();
        find_function("main", null);

        //RUN CODE
        for (; line_numb < code.Count; line_numb++)
        {
            eval_line(code[line_numb]);
            steps++;
        }

    }

    public static void debug_vomit_code()
    {
        int i = 1;
        foreach (string line in code)
        {
            Console.WriteLine(i + ": " + line);
            i++;
        }
    }
    public static void debug_vomit_steps() {
        Console.WriteLine("Curently on step " + steps);
    }

    public static void debug_vomit_sectors()
    {
        foreach (var sector in sectors)
        {
            Console.WriteLine(sector.Key + ": " + sector.Value.Item1 + " - " + sector.Value.Item2);
        }
    }

    public static void interpeter_settings_base_setup()
    {
        vars.Add("setting_dps", "0".ToCharArray());
        vars.Add("setting_caching_out", "0".ToCharArray());
    }

    public static string calc_display_line(bool other_info)
    {
        foreach (var sector in sectors)
        {
            (int, int) sector_start_end = sector.Value;
            if (line_numb >= sector_start_end.Item1 && line_numb <= sector_start_end.Item2)
            {
                return ((line_numb - sector_start_end.Item1) + (sector.Key == "BASE" ? 0 : 1)) + (other_info ? " in sector \"" + sector.Key + "\"" : null);
            }
        }
        return null;
    }
    public static bool find_sectors(List<string> tokens, int i)
    {
        if (tokens[0] == "SECSTART")
        {
            tokens.RemoveAt(0);
            if (!sectors.ContainsKey(tokens[0]))
            {
                sectors.Add(tokens[0], (i, 0));
                last_added_sector = tokens[0];
                return true;
            }
            else
            {
                return false;
            }
        }
        if (tokens[0] == "SECEND")
        {
            tokens.RemoveAt(0);
            if (sectors[tokens[0]].Item2 == 0)
            {
                int sector_start = sectors[tokens[0]].Item1;
                sectors[tokens[0]] = (sector_start, i);
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public static void eval_line(string line)
    {

        List<string> tokens = new List<string>();
        line = line.Trim();
        string pattern = "\\s*//.*";
        line = Regex.Replace(line, pattern, "");
        tokens = line.Split(" ").ToList();
        
        if (tokens.Count <= 0) { }
        else
        {
            switch (new string(vars["setting_dps"]))
            {
                case "0":
                case "00":
                case "000":
                    break;
                case "10":
                case "1":
                case "100":
                    Console.WriteLine(calc_display_line(false) + ": ");
                    break;
                case "01":
                case "010":
                    Console.WriteLine(calc_display_line(true) + ": ");
                    break;
                case "11":
                case "110":
                    Console.Write(calc_display_line(true) + ": ");
                    Console.WriteLine(tokens[0]);
                    break;
                case "001":
                    if (tokens.Count > 1 && !tokens[0].Contains("//"))
                    {
                        Console.Write(calc_display_line(true) + ": ");
                        foreach (string token in tokens)
                        {
                            Console.Write(token + " ");
                        }
                        Console.WriteLine();
                    }
                    break;
                case "101":
                    Console.Write(calc_display_line(true) + ": ");
                    foreach (string token in tokens)
                    {
                        Console.Write(token + " ");
                    }
                    Console.WriteLine();
                    break;
            }
            // CALCULATING
            if (tokens[0] == "SET")
            {
                tokens.RemoveAt(0);
                tokenize_set(tokens);
            }
            // CREATING A VARIABLE
            if (tokens[0] == "VAR")
            {
                tokens.RemoveAt(0);
                tokenize_var(tokens, false);

            }
            // BINARY OPERATION
            if (tokens[0] == "OPERATION")
            {
                tokens.RemoveAt(0);
                tokenize_operation(tokens);
            }
            if (tokens[0] == "SKIP" || tokens[0] == "NSKIP") {
                
                bool passed = tokens[0] == "SKIP";
                tokens.RemoveAt(0);
                int lines_to_skip = int.Parse(tokens[0]);
                while (tokens.Count > 1)
                {
                    try
                    {
                        if (tokens[1] == "IF")
                        {
                            if (get_var(tokens[2], false, true)[(get_var(tokens[3], false, false) == null ? int.Parse(tokens[3]) : Convert.ToUInt32(new string(reverse_array(get_var(tokens[3], false, false))), 2))] == '1')
                            //if (find_var(tokens[2], false, true)[tokens[2]][0] == '1')
                            {
                                tokens.RemoveRange(1, 3);
                            }
                            else
                            {
                                passed = !passed;
                                break;
                            }


                        }
                        else if (tokens[1] == "NOT")
                        {
                            if (get_var(tokens[2], false, true)[(get_var(tokens[3], false, false) == null ? int.Parse(tokens[3]) : Convert.ToUInt32(new string(reverse_array(get_var(tokens[3], false, false))), 2))] == '0')
                            //if (find_var(tokens[2], false, true)[tokens[2]][int.Parse(tokens[3])] == '0')
                            {
                                tokens.RemoveRange(1, 3);
                            }
                            else { passed = !passed; break; }
                        }
                    } catch (Exception e)
                    {
                        passed = !passed; break;
                        
                    }
                }
                if (passed)
                {
                    line_numb += lines_to_skip;
                }
            }
            if (tokens[0] == "VOMIT")
            {
                if (tokens[1] == "CODE")
                {
                    debug_vomit_code();
                }
                if (tokens[1] == "SECTORS")
                {
                    debug_vomit_sectors();
                }
                if (tokens[1] == "STEPS") {
                    debug_vomit_steps();
                }
            }
            if (tokens[0] == "PRINT")
            {
                tokens.RemoveAt(0);
                tokenize_print(tokens);
            }
            if (tokens[0] == "PRINTLN")
            {
                tokens.RemoveAt(0);
                tokenize_print(tokens);
                Console.WriteLine();
            }
            if (tokens[0] == "LN")
            {
                Console.WriteLine();
            }
            if (tokens[0] == "GLOBAL")
            {
                tokens.RemoveAt(0);
                tokenize_var(tokens, true);
            }
            if (tokens[0] == "USE")
            {
                tokens.RemoveAt(0);
                string where_ret = tokens[0];
                bool passed = true;
                while (tokens.Count > 3)
                {
                    try
                    {
                        if (tokens[3] == "IF")
                        {

                            if (get_var(tokens[4], false, true)[(get_var(tokens[5], false, false) == null ? int.Parse(tokens[5]) : Convert.ToUInt32(new string(reverse_array(get_var(tokens[5], false, false))), 2))] == '1')

                            //if (find_var(tokens[4], false, true)[tokens[4]][int.Parse(tokens[5])] == '1')
                            {
                                tokens.RemoveRange(3, 3);
                            }
                            else
                            {
                                passed = false;
                                break;
                            }
                        }
                        else if (tokens[3] == "NOT")
                        {
                            if (get_var(tokens[4], false, true)[(get_var(tokens[5], false, false) == null ? int.Parse(tokens[5]) : Convert.ToUInt32(new string(reverse_array(get_var(tokens[5], false, false))), 2))] == '0')

                            {
                                tokens.RemoveRange(3, 3);
                            }
                            else
                            {
                                passed = false;
                                break;
                            }
                        }
                    } catch (Exception e)
                    {
                        passed = false; break;
                    }
                }
                if (passed) {
                    find_function(tokens[2], where_ret);
                }

            }
            if (tokens[0] == "ASSIGN")
            {
                tokens.RemoveAt(0);
                assign_var(tokens);
            }
            if (tokens[0] == "END")
            {
                if (functions.Count > 1)
                {
                    if (tokens.Count > 1)
                    {
                        Function judged_func = functions[functions.Count - 1];
                        char[] ret_val = (char[])get_var(tokens[1], false, true).Clone();
                        judged_func.add_return_val(ret_val);
                        string func_identifier = judged_func.get_identifier();
                        set_var(judged_func.where_ret, true, true)[judged_func.where_ret] = ret_val;
                        if (!cached_functions.ContainsKey(func_identifier ) && func_identifier != "main" && func_identifier != "setup") {
                            cached_functions.Add(func_identifier ,judged_func);
                            if (new string(vars["setting_caching_out"]) == "1" || new string(vars["setting_caching_out"]) == "01"|| new string(vars["setting_caching_out"]) == "001")
                            {
                                Console.WriteLine("Caching function: \"" + func_identifier + "\" with return value: \"" + new string(ret_val) + "\".");
                            }
                        }
                    }

                    line_numb = functions[functions.Count - 1].called_from;
                    functions.RemoveAt(functions.Count - 1);
                }
                else
                {
                    //OnApplicationExit(null, null);
                    Environment.Exit(0);
                }
            }
            if (tokens[0] == "INVOKE")
            {
                tokens.RemoveAt(0);
                bool passed = true;
                while (tokens.Count > 1)
                {
                    if (tokens[1] == "IF")
                    {
                        if (get_var(tokens[2], false, true)[(get_var(tokens[3], false, false) == null ? int.Parse(tokens[3]) : Convert.ToUInt32(new string(reverse_array(get_var(tokens[3], false, false))), 2))] == '1')
                       
                        {
                            tokens.RemoveRange(1, 3);
                        } else
                        {
                            passed = false;
                            break;
                        }


                    } else if (tokens[1] == "NOT")
                    { 
                        if (get_var(tokens[2], false, true)[(get_var(tokens[3], false, false) == null ? int.Parse(tokens[3]) : Convert.ToUInt32(new string(reverse_array(get_var(tokens[3], false, false))), 2))] == '0')
                       
                        {
                            tokens.RemoveRange(1, 3);
                        }
                        else { passed = false; break; }
                    }
                }
                if (passed)
                {
                    find_function(tokens[0], null);
                }
            }
        }
    }


    public static string find_previous_function() {
        for (int i = line_numb; i <= 0; i--) {
            string[] tokens = code[i].Trim().Split(" ");
            if (tokens[0] == "FUNCTION") { 
                return tokens[1];
            }
        }
        return null;
    }
    public static void assign_var(List<string> tokens)
    {
        char[] reverse = tokens[1].ToCharArray();
        Array.Reverse(reverse);
        set_var(tokens[0], false, true)[tokens[0]] = reverse;
    }
    public static void include()
    {
        for (int i = 0; i < code.Count; i++)
        {
            string line = code[i];
            line = line.Trim();
            string pattern = "\\s*//.*";
            line = Regex.Replace(line, pattern, "");
            List<string> tokens = line.Split(" ").ToList();
            find_sectors(tokens, i);
            if (tokens[0] == "INCLUDE")
            {
                tokens.RemoveAt(0);
                List<string> included_lines = File.ReadAllLines("C:/aha/" + tokens[0]).ToList<string>();
                bool include = true;
                int enumerator = code.Count + 1;
                foreach (string new_line in included_lines)
                {

                    if (new_line.Contains("SECSTART") || new_line.Contains("SECEND"))
                    {
                        include = find_sectors(new_line.Split(' ').ToList<string>(), enumerator);
                    }
                    enumerator++;
                }
                if (include)
                {
                    foreach (string line_of_include in included_lines)
                    {
                        code.Add(line_of_include);
                    }
                    find_function(last_added_sector + ":setup", null);
                }
            }
        }

    }
    public static char[] reverse_array(char[] originalString)
    {
        char[] reversedCharArray = new char[originalString.Length];
        for (int i = 0; i < originalString.Length; i++)
        {
            reversedCharArray[i] = originalString[originalString.Length - 1 - i];
        }
        return reversedCharArray;
    }
    public static void tokenize_set(List<string> tokens)
    {
        string target_name = tokens[0];
        int target_start;
        if (get_var(tokens[1], false, false) != null)
        {
            target_start = Convert.ToInt32(new string(reverse_array(get_var(tokens[1], false, true))), 2);
        } else
        {
            target_start = int.Parse(tokens[1]);
        }
        
        string setter_name = tokens[2];
        int setter_start;
        if (get_var(tokens[3], false, false) != null)
        {
            setter_start = Convert.ToInt32(new string(reverse_array(get_var(tokens[3], false, true))), 2);
        }
        else
        {
            setter_start = int.Parse(tokens[3]);
        }
        int setter_len;
        if (tokens.Count < 5)
        {
            setter_len = 1;
        }
        else
        {
            if (get_var(tokens[4], false, false) != null)
            {
                setter_len = Convert.ToInt32(new string(reverse_array(get_var(tokens[4], false, true))), 2);
            }
            else
            {
                setter_len = int.Parse(tokens[4]);
            }
        }
        int setter_index = setter_start;
        int target_index = target_start;
        for (int i = 0; i < setter_len; i++)
        {
            get_var(target_name, false, true)[target_index + i] = get_var(setter_name, false, true)[setter_index + i];
        }
    }

    public static void find_function(string name, string where_ret)
    {
        int i = 0;
        int last_iteration = code.Count;
        if (name.Contains(":"))
        {
            name = name.Split(':')[1];
            string sector_name = name.Split(':')[0];
            if (sectors.ContainsKey(sector_name))
            {
                (i, last_iteration) = sectors[sector_name];

            }
        }
        bool found = false;
        for (; i < last_iteration; i++)
        {
            string line = code[i];
            line = line.Trim();
            string pattern = "\\s*//.*";
            line = Regex.Replace(line, pattern, "");
            List<string> tokens = line.Split(" ").ToList();
            if (tokens[0] == "FUNCTION")
            {
                Dictionary<string, char[]> parameters = new Dictionary<string, char[]>();
                if (tokens[1].Split("(")[0] == name.Split("(")[0])
                {
                    if (name.Contains("("))
                    {
                        string params_to_parse_from = name.Split("(")[1];
                        string[] parameter_names = params_to_parse_from.Substring(0, params_to_parse_from.Length - 1).Split(",");
                        string params_to_parse_to = tokens[1].Split("(")[1];
                        string[] parameter_names_to = params_to_parse_to.Substring(0, params_to_parse_to.Length - 1).Split(",");


                        for (int j = 0; j < parameter_names.Length; j++)
                        {
                            parameters.Add(parameter_names_to[j], get_var(parameter_names[j], false, true));
                        }
                    }
                        string identifier = ""; int len = 0;
                    identifier += name.Split("(")[0] + ":";
                    foreach (var param in parameters)
                        {
                            char[] char_val = (char[])param.Value.Clone();
                            identifier += new string(char_val);
                            if (len < parameters.Count - 1)
                            {
                                identifier += ",";
                            }
                            len++;
                        }
                    if (!cached_functions.ContainsKey(identifier))
                    {
                        functions.Add(new Function(name, i, line_numb, parameters, where_ret));
                        line_numb = i;
                    } else
                    {
                        set_var(where_ret, false, true)[where_ret] = cached_functions[identifier].return_val;
                        if (new string(vars["setting_caching_out"]) == "1" || new string(vars["setting_caching_out"]) == "01" || new string(vars["setting_caching_out"]) == "001")
                        {
                            Console.WriteLine("Using cached function: \"" + identifier + "\" with return value: \"" + new string(cached_functions[identifier].return_val) + "\".");
                        }
                        
                    }
                    found = true;
                }
            }
        }
        if (!found)
        {
            Console.Error.WriteLine("Could not find a function \"" + name + "\" requested on " + calc_display_line(true) + ".");
            Environment.Exit(1);
        }
    }

    public static void tokenize_print(List<string> tokens)
    {
        string var_name = tokens[0];
        string var_value;
        var_value = new string(get_var(var_name, false, true));

        if (tokens.Count > 1)
        {
            print(var_value, tokens[1]);
        }
        else {
            print(var_value, null);
        }
    }

    public static void print(string text, string property)
    {
        char[] reverse = text.ToCharArray();
        Array.Reverse(reverse);
        
        
        text = new string(reverse);
        if (property == "TRIM") {
            while (text.StartsWith("0") && text.Length > 1)
            {
                text = text.Substring(1);
            }
        }
        if (property == "DECIMAL")
        {
            text = Convert.ToInt64(text, 2).ToString();
        }
        if (property == "UDECIMAL")
        {
            text = Convert.ToUInt64(text, 2).ToString();
        }
        Console.Write(text);
    }

    public static void tokenize_operation(List<string> tokens)
    {
        string target_name = tokens[0];
        char operand_one = '!';
        char operand_two = '!';
        int operation = 2;
        if (tokens[1] != "IS") { Console.Error.WriteLine("No set operator used on line " + calc_display_line(true) + "!"); Environment.Exit(1); }
        //NOT
        if (tokens[2] == "NOT")
        {
            operation = 0;
            string operand_name = tokens[3];
            operand_one = get_var(operand_name, false, true)[0];

        }
        //AND
        if (tokens[3] == "AND")
        {
            operation = 1;
            string operand_one_name = tokens[2];
            string operand_two_name = tokens[4];
            operand_one = get_var(operand_one_name, false, true)[0];
            operand_two = get_var(operand_two_name, false, true)[0];

        }
        solve_operation(target_name, operand_one, operand_two, operation, tokens[3]);
    }

    public static bool is_var_global(string name) {
        if (vars.ContainsKey(name))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static char[] get_var(string name, bool deeper, bool error)
    {


        if (vars.ContainsKey(name))
        {
            return vars[name];
        }
        if (deeper)
        {
            for (int i = functions.Count - 2; i >= 0; i--)
            {
                if (functions[i].parameters.ContainsKey(name))
                {
                    return functions[i].parameters[name];
                }
            }
        }
        else
        {
            if (functions[functions.Count - 1].parameters.ContainsKey(name))
            {
                return functions[functions.Count - 1].parameters[name];
            }
        }
        if (error)
        {
            Console.Error.WriteLine("Variable \"" + name + "\", requested on line " + calc_display_line(true) + " in function \"" + functions[functions.Count - 1].name + "\" does not exist in global or local scope.");
            Environment.Exit(1);
        }
        return null;
    }
    public static Dictionary<string, char[]> set_var(string name, bool deeper, bool error)
    {


        if (vars.ContainsKey(name))
        {
            return vars;
        }
        if (deeper)
        {
            for (int i = functions.Count - 2; i >= 0; i--)
            {
                if (functions[i].parameters.ContainsKey(name))
                {
                    return functions[i].parameters;
                }
            }
        }
        else
        {
            if (functions[functions.Count - 1].parameters.ContainsKey(name))
            {
                return functions[functions.Count - 1].parameters;
            }
        }
        if (error)
        {
            Console.Error.WriteLine("Variable \"" + name + "\", requested on line " + calc_display_line(true) + " in function \"" + functions[functions.Count - 1].name + "\" does not exist in global or local scope.");
            Environment.Exit(1);
        }
        return null;
    }

    public static void solve_operation(string target_name, char operand_one, char operand_two, int operation, string operator_name)
    {
        if (operation == 0)
        {

            get_var(target_name, false, true)[0] = operand_one == '1' ? '0' : '1';

        }
        else if (operation == 1)
        {

            get_var(target_name, false, true)[0] = (operand_one == '1' && operand_two == '1') ? '1' : '0';
        }
        else
        {
            Console.Error.WriteLine("Unknown operator \"" + operator_name + "\" on line " + calc_display_line(true));
            Environment.Exit(1);
        }
    }

    /**
     * 
     * VAR name[len]
     * 
     * VAR name[len] = val
     * 
     * */
    public static void tokenize_var(List<string> tokens, bool global)
    {
        string name_len = tokens[0];
        int first_par, second_par;
        first_par = name_len.IndexOf('[');
        second_par = name_len.IndexOf("]");
        string var_name = name_len.Substring(0, first_par);
        int size = int.Parse(name_len.Substring(first_par + 1, second_par - (first_par + 1)));
        //ERROR HANDLING
        if (size <= 0) { Console.Error.WriteLine("Buffer size for \"" + var_name + "\" is set to 0 or lower on line " + calc_display_line(true) + "!"); Environment.Exit(1); }
        char[] value = new char[size];
        if (tokens.Count() != 1)
        {
            //ERROR HANDLING
            if (tokens[2].Length != size) { Console.Error.WriteLine("Buffer size and value length for \"" + var_name + "\" do not match on line " + calc_display_line(true) + "!"); Environment.Exit(1); }

            value = tokens[2].Trim().ToCharArray();
            Array.Reverse(value);
            foreach (char c in value)
            {
                if (c != '1' && c != '0')
                {
                    Console.Error.WriteLine("Variables only take binary input, instead of \"" + value + "\" on line " + calc_display_line(true));
                    Environment.Exit(1);
                }
            }
        }

        make_var(var_name, value, global);


    }

    public static void make_var(string name, char[] value, bool global)
    {
        if (global)
        {

            //ERROR HANDLING
            if (vars.ContainsKey(name)) { Console.Error.WriteLine("Variable \"" + name + "\" on line " + calc_display_line(true) + " already exists as a global variable."); Environment.Exit(1); }
            foreach (Function func in functions)
            {
                foreach (string var_name in func.parameters.Keys)
                {
                    if (var_name == name)
                    {
                        Console.Error.WriteLine("Variable \"" + name + "\" on line " + calc_display_line(true) + " already exists in function \"" + func.name + "\"."); Environment.Exit(1);
                    }
                }
            }

            vars.Add(name, value);
        }
        else
        {
            if (vars.ContainsKey(name)) { Console.Error.WriteLine("Variable \"" + name + "\" on line " + calc_display_line(true) + " already exists as a global variable."); Environment.Exit(1); }
            if (functions[functions.Count - 1].parameters.ContainsKey(name)) { Console.Error.WriteLine("Variable \"" + name + "\" on line " + calc_display_line(true) + " already exists in this function."); Environment.Exit(1); }
            functions[functions.Count - 1].parameters.Add(name, value);
        }

    }
}