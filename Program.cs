using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Proj
{
    class Program
    {
        static List<contributor> contributor_list = new List<contributor>();
        static List<project> project_list = new List<project>();
        static string path = @"put\";
        static List<string> file_names = new List<string>();
        static int last_day = 0;


        static void Main(string[] args)
        {
            // file_names.Add("a_an_example.in.txt");
            // file_names.Add("b_better_start_small.in.txt");
            // file_names.Add("c_collaboration.in.txt");
            // file_names.Add("d_dense_schedule.in.txt");
            // file_names.Add("e_exceptional_skills.in.txt");
            file_names.Add("f_find_great_mentors.in.txt");
            foreach (var file in file_names)
            {

                ReadInputFiles(path + file);
                executed_project output_executed_project = new executed_project();
                //solution
                output_executed_project = Solution();
                WriteOutput(output_executed_project, path + file);
            }
        }
        static executed_project Solution()
        {
            executed_project output_executed_project = new executed_project();

            int time_passed = 0;
            int prev_time_passed = -1;
            bool time_loop = false;
            do{
                if(time_passed==prev_time_passed)
                time_loop = true;
                else{
                    prev_time_passed = time_passed;
                }


            var lst = project_list.Where(p=>p.is_executed==false).GroupBy(p => p.proj_best_before_day).Select(p => new { proj_best_before_day = p.Key, list_proj = p.ToList() }).ToList();

            foreach (var best_before_projs in lst)
            {
                int executed_proj_time = 0;
                foreach (var proj in best_before_projs.list_proj)
                {
                    executed_project_info my_executed_project_info = new executed_project_info();
                    my_executed_project_info.cont_name_list = new List<string>(new string[proj.proj_skills.Count()]).ToList();


                    foreach (var p_skill in proj.proj_skills)
                    {
                        var selected_person = contributor_list.Where(n => n.is_available == true &&
                        n.cont_skills.Where(p => p.skill_level == p_skill.skill_level && p.skill_name == p_skill.skill_name).Count() > 0).FirstOrDefault();

                        if (selected_person != null)
                         {   contributor_list = contributor_list.Select(p =>
                            {
                                if (p.cont_name == selected_person.cont_name)
                                {
                                    p.is_available = false;
                                    p.can_upgrade = true;
                                    p.skill_to_upgrade = p_skill;
                                }
                                return p;

                            }).ToList();

                        my_executed_project_info.cont_name_list[p_skill.skill_id] = selected_person.cont_name;
                    }
                    // if no one has exact level, get higher level person
                    selected_person = contributor_list.Where(n => n.is_available == true &&
                        n.cont_skills.Where(p => p.skill_level > p_skill.skill_level && p.skill_name == p_skill.skill_name).Count() > 0).FirstOrDefault();

                        if (selected_person != null)
                         {   contributor_list = contributor_list.Select(p =>
                            {
                                if (p.cont_name == selected_person.cont_name)
                                {
                                    p.is_available = false;
                                }
                                return p;

                            }).ToList();

                        my_executed_project_info.cont_name_list[p_skill.skill_id] = selected_person.cont_name;
                    }
                    }
                    if (proj.n_skill == my_executed_project_info.cont_name_list.Where(p => p != "" && p != null).Count())
                    {
                        if (proj.proj_duration_day > executed_proj_time)
                            executed_proj_time = proj.proj_duration_day;

                        project_list = project_list.Select(p =>
                        {
                            if (p.proj_name == proj.proj_name)
                            {
                                p.is_executed = true;
                            }
                            return p;
                        }).ToList();

                        // upgrade skill
                        contributor_list = contributor_list.Select(p =>
                        {
                            if (my_executed_project_info.cont_name_list.Contains(p.cont_name) && p.can_upgrade == true)
                            {
                                p.cont_skills = p.cont_skills.Select(c =>
                                {
                                    if (c.skill_name == p.skill_to_upgrade.skill_name && c.skill_level == p.skill_to_upgrade.skill_level)
                                    { 
                                        c.skill_level = c.skill_level+1; 

                                        }
                                    return c;
                                }).ToList();
                                
                                p.can_upgrade = false;
                            }
                            return p;

                        }).ToList();
                        

                            my_executed_project_info.proj_name = proj.proj_name;
                            my_executed_project_info.cont_name_list = my_executed_project_info.cont_name_list;

                            output_executed_project.executed_Project_Infos.Add(my_executed_project_info);

                        // if proj done executing, release ppl
                        contributor_list = contributor_list.Select(p =>
                        {
                            if (my_executed_project_info.cont_name_list.Contains(p.cont_name))
                            {
                                p.is_available = true;
                            }
                            return p;

                        }).ToList();
                    }
                    else
                    {
                        // if pro not executed, release ppl
                        contributor_list = contributor_list.Select(p =>
                        {
                            if (my_executed_project_info.cont_name_list.Contains(p.cont_name))
                            {
                                p.is_available = true;
                            }
                            return p;

                        }).ToList();
                    }

                    // if any proj executed, time should move
                    time_passed += executed_proj_time;
                    Console.WriteLine("Day #"+time_passed+"/"+last_day);
                    
                }

            }

            
                }while(time_passed<last_day&& time_loop==false);

                output_executed_project.n_executed_project = output_executed_project.executed_Project_Infos.Count();
            return output_executed_project;
        }

        //Not tested
        static void WriteOutput(executed_project output, string full_path)
        {

            string output_info = output.n_executed_project.ToString();

            foreach (var item in output.executed_Project_Infos)
            {
                output_info += "\n" + item.proj_name+"\n";
                foreach (var role in item.cont_name_list)
                {
                    output_info += role+" ";
                }

            }

            File.WriteAllText(full_path + "_output.txt", output_info);
            Console.WriteLine(full_path + " - # of projects " + output.n_executed_project);

        }
        static void ReadInputFiles(string full_path)
        {
            var my_input = File.ReadAllLines(full_path);
            var firstline = my_input[0].Split(' ');
            int contributor_lines = int.Parse(firstline[0]);
            for (int i = 1; i <= contributor_lines; i++)
            {
                contributor my_cont = new contributor();
                var cont_info = my_input[i].Split(' ');
                my_cont.cont_name = cont_info[0];
                my_cont.n_cont_skills = int.Parse(cont_info[1]);

                for (int j = i + 1; j <= i + my_cont.n_cont_skills; j++)
                {
                    var skill_line = my_input[j].Split(' ');

                    skill cont_skill = new skill();
                    cont_skill.skill_name = skill_line[0];
                    cont_skill.skill_level = int.Parse(skill_line[1]);

                    my_cont.cont_skills.Add(cont_skill);
                }
                i = i + my_cont.n_cont_skills;
                contributor_lines += my_cont.n_cont_skills;
                contributor_list.Add(my_cont);
            }

            int project_lines = int.Parse(firstline[1]);
            for (int i = contributor_lines + 1; i <= project_lines + contributor_lines; i++)
            {
                var proj_line = my_input[i].Split(' ');
                project proj = new project();

                proj.proj_name = proj_line[0];
                proj.proj_duration_day = int.Parse(proj_line[1]);
                proj.proj_score = int.Parse(proj_line[2]);
                proj.proj_best_before_day = int.Parse(proj_line[3]);
                if(proj.proj_best_before_day>last_day)
                last_day = proj.proj_best_before_day;
                proj.n_skill = int.Parse(proj_line[4]);


                for (int j = i + 1; j <= i + proj.n_skill; j++)
                {
                    var skill_line = my_input[j].Split(' ');

                    skill proj_skill = new skill();
                    // Test
                    proj_skill.skill_id = j - i - 1;

                    proj_skill.skill_name = skill_line[0];
                    proj_skill.skill_level = int.Parse(skill_line[1]);

                    proj.proj_skills.Add(proj_skill);
                }
                i = i + proj.n_skill;
                project_lines += proj.n_skill;
                project_list.Add(proj);

            }
            Console.WriteLine("# of contributors= " + contributor_list.Count() + "\n" +
            "# of projects= " + project_list.Count());


        }
    }

    class contributor
    {
        public string cont_name { get; set; }
        public int n_cont_skills { get; set; }

        public bool is_available { get; set; } = true;
        public bool can_upgrade { get; set; } = false;
        public skill skill_to_upgrade { get; set; }

        public List<skill> cont_skills { get; set; } = new List<skill>();
    }
    class skill
    {
        public int skill_id { get; set; }
        public string skill_name { get; set; }
        public int skill_level { get; set; }
        public bool is_available { get; set; } = true;
    }

    class project
    {
        public string proj_name { get; set; }
        public int proj_duration_day { get; set; }
        public int proj_score { get; set; }
        public int proj_best_before_day { get; set; }

        public int n_skill { get; set; }
        public bool is_executed { get; set; } = false;

        public List<skill> proj_skills { get; set; } = new List<skill>();

    }

    class executed_project
    {
        public int n_executed_project { get; set; }
        public List<executed_project_info> executed_Project_Infos { get; set; } = new List<executed_project_info>();

    }
    class executed_project_info
    {
        public string proj_name { get; set; }
        public List<string> cont_name_list { get; set; } = new List<string>();
    }


}
