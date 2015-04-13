using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Worker_JobTracker
{
    class Program
    {
        private static String ip;
        private static String id;
        private static String client_ip;
        private static List<Worker> workers_list;
        
        /************************
         * METODOS PARA O JOB TRACKER
        ************************/
        // This is the call that the AsyncCallBack delegate will reference.
        public void assignWorkers(int split_number)
        {
            
            int[] split = new int[split_number];
            List<Worker> workers_list_sorted = workers_list.OrderBy(o=>o.rank).ToList();//Sort List of workers

            //iterate through workers
            foreach (var w in workers_list) {
                Console.WriteLine("Amount is {0} and type is {1}", w.id, w.ip);
            }
            

            if(workers_list.Count > split_number){//+ workers do que splits

                for (int i = 0; i < split_number; i++ )
                {
                    informWorker(workers_list[i], i);
                }

            }else if(workers_list.Count > split_number){//+ splits do que workers



            }else{//igual numero entre splits e workers

                   
            }        
     
        }

        //informa o worker que deve executar o trabalho (este de correr o metodo execute Job);
        private void informWorker(Worker worker, int split_id)
        {

        }








        private void informJoin(Worker worker)
        {
            foreach (var w in workers_list)
            {
                //send message to w informing that
            }


        }





        /************************
        * METODOS PARA O WORKER
        ************************/
        //comunica com a aplicacao cliente a pedir o trabalho que lhe foi atribuido pelo Job Tracker
        private String getJob(Job job)
        {
            String split = "";
            return split;
        }

        //comunica com a aplicacao cliente a pedir o trabalho que lhe foi atribuido pelo Job Tracker
        private void sendJob(String job_output)
        {

        }

        //executa trabalho
        private void execute_job(Job job)
        {
            String split = getJob(job);

            //codigo para tratar o split

            String job_output = "";
            sendJob(job_output);
        }







        static void Main(string[] args)
        {
            //Criar server a agardar comunicação dos workers
            workers_list.Add(new Worker("192.168.1.1", "1"));
            workers_list.Add(new Worker("192.168.1.1", "2"));
            workers_list.Add(new Worker("192.168.1.1", "3"));

            int split_number = 2;
            assignWorkers(split_number);

        }
    }







    /************************
     * Classes
    **************************/
    class Worker
    {
        public double rank;//classificação de fiabilidade (contactados primeiro)
        public String ip;
        public String id;
        public bool ready;

        public Worker(String ip, String id){

            this.ip = ip;
            this.id = id;
            ready = true;
  
        }
    }

    class Job
    {
        public Worker worker;//classificação de fiabilidade (contactados primeiro)
        public int splitId;
        public String state;//waiting for worker, waiting for split, in progress, finished, aborted

        public Job(Worker worker, int splitId, String state)
        {

            this.worker = worker;
            this.splitId = splitId;
            this.state = state;
        }
    }

}
