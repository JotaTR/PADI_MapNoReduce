using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

/// CHANGE TO CLIENT: Renato


namespace PADI_MapNoReduce
{
  
    /************************
     * Classes
    **************************/
    [Serializable]
    public class Worker
    {
        public double timePerTask;//classificação de fiabilidade (contactados primeiro)
        public String address;
        public int id;
        public bool replica;
        public bool ready;//true -> Pode receber novas Tasks; false -> Está ocupado com uma task

        public Worker(String address, int id, bool replica)
        {

            this.address = address;
            this.id = id;
            this.replica = replica;
            ready = true;

        }

 //       public Worker(String workerString)
 //       {
 //           string[] attributes = workerString.Split(new string[] { ";" }, StringSplitOptions.None);
 //           
 //           this.timePerTask = double.Parse(attributes[0], System.Globalization.CultureInfo.InvariantCulture);
 //           this.address = attributes[1];
 //           this.id = Int32.Parse(attributes[2]); 
 //           
 //           if (attributes[3] == "false")
 //           {
 //               this.replica = false;
 //           }
 //           else
 //           {
 //               this.replica = true;
 //           }
 //
 //           if (attributes[4] == "false")
 //           {
 //               this.ready = false;
 //           }
 //           else
 //           {
 //               this.ready = true;
 //           }
 //       }
 //
        public String toString()
        {

            String workerString;
            workerString = String.Concat(this.timePerTask, ";", this.address, ";", this.id, ";");
            if(this.replica == false)
            {
                workerString = workerString + "false;";
            }else{
                workerString = workerString + "true;";
            }

            if(this.ready == false)
            {
                workerString = workerString + "false";
            }else{
                workerString = workerString + "true";
            }

            return workerString;
        }

    }

    [Serializable]
    public class JobTracker
    {
        public String address;
        public int id;
        public int nbr_worker;

        public JobTracker(String address, int id)
        {
            this.address = address;
            this.id = id;
            this.nbr_worker = 0;
        }

//        public JobTracker(String jobTrackerString)
//        {
//            string[] attributes = jobTrackerString.Split(new string[] { ";" }, StringSplitOptions.None);
//            this.address = attributes[0];
//            this.id = Int32.Parse(attributes[1]);
//            this.nbr_worker = Int32.Parse(attributes[2]);            
//        }
//
        public String toString()
        {

            String workerString = String.Concat(this.address, ";", this.id, ";", this.nbr_worker, ";");
            return workerString;
        }

        public void incrementWorker(){
            nbr_worker++;
        }

        public void decrementWorker()
        {
            if (nbr_worker > 0)
                nbr_worker--;
            else
                nbr_worker = 0;
        }
    }

    [Serializable]
    public class WorkerState
    {
        public bool ready;//verifica se o nó nestá livre ou está a trabalhar
        public bool freeze;//permite para a execução do node
        public int tasks_remaining;

        public WorkerState(bool ready, bool freeze, int tasks_remaining)
        {
            this.ready = ready;
            this.freeze = freeze;
            this.tasks_remaining = tasks_remaining;
        }
    }

    [Serializable]
    public class SubJobW
    {
     
        public List<int> taskList;
        public int workerId;
        public int jobTrackerId;
        public String clientAddress;
        public String text_file;
        public int starting_unixTimeStamp;
        public int initial_task_nbr;

        public SubJobW(int workerId, int jobTrackerId, String clientAddress, String text_file, List<int> taskList)
        {
            this.taskList = taskList;
            this.workerId = workerId;
            this.jobTrackerId = jobTrackerId;
            this.clientAddress = clientAddress;
            this.text_file = text_file;
            this.initial_task_nbr = taskList.Count;
            this.starting_unixTimeStamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

 //       public SubJobW(String subJobWString)
 //       {
 //  
 //           string[] attributes = subJobWString.Split(new string[] { ";" }, StringSplitOptions.None);
 //
 //           //cria taskList
 //           string[] taskListString = attributes[0].Split(new string[] { "," }, StringSplitOptions.None);
 //           this.taskList = new List<int>();
 //           foreach (String taskString in taskListString)
 //           {
 //               taskList.Add(Int32.Parse(taskString));
 //           }
 //           this.workerId = Int32.Parse(attributes[1]);
 //           this.jobTrackerId = Int32.Parse(attributes[2]);
 //           this.clientAddress = attributes[3];
 //           this.text_file = attributes[4];
 //           this.starting_unixTimeStamp = Int32.Parse(attributes[5]);
 //           this.initial_task_nbr = Int32.Parse(attributes[6]);
 //
 //       }
 //
        public String toString()
        {
            
            String workerString = "";
            int counter = 1;
            foreach (int task in taskList)
            {
                if (counter == taskList.Count)
                {
                    workerString = String.Concat(workerString, task, ";");
                    counter++;
                }
                else
                {
                    workerString = String.Concat(workerString, task, ",");
                    counter++;
                }
            }
            workerString = String.Concat(workerString, ";", this.workerId, ";", this.jobTrackerId, ";", this.clientAddress, ";", this.text_file, ";", this.starting_unixTimeStamp, ";", this.initial_task_nbr);
            
            return workerString;
        }
    }

    [Serializable]
    public class SharedClass
    {
        public byte[] code; 
        public String className;
        public String split;

        public SharedClass(byte[] code, String split, String className)
        {
            this.code = code;
            this.split = split;
            this.className = className;
        }

    }

    public class JobArguments
    {

        public int nbr_splits;
        public String clientAddress;
        public String text_file;
        public String address;
        public int startingSplit_nbr;

        public JobArguments(int nbr_splits, String address, String clientAddress, String text_file, int startingSplit_nbr)
        {
            this.address = address;
            this.nbr_splits = nbr_splits ;
            this.clientAddress = clientAddress;
            this.text_file = text_file;
            this.startingSplit_nbr = startingSplit_nbr;
        }
    }

    public class SubJobArguments
    {

        public String clientAddress;
        public String text_file;
        public String address;
        public List<int> task_list;
        public int workerId;
        public int jobTrackerId;

        public SubJobArguments(int workerId, int jobTrackerId, String address, String clientAddress, String text_file, List<int> task_list)
        {
            this.address = address;
            this.clientAddress = clientAddress;
            this.text_file = text_file;
            this.task_list = task_list;
            this.workerId = workerId;
            this.jobTrackerId = jobTrackerId;

        }
    }
    
    //Worker + Replica Interface
    public interface WorkerInterfaceRef
    {

        /**********************
         * JOB TRACKER INTERFACE
        **********************/
        //Permite que o cliente submeta um novo Job
        String submitJobService(int split_number, String client_address, String text_file);
        //Permite que o Job seja dividido por vários JobTrackers
        void submitSubJobService(int split_number, String client_address, String text_file, int startingSplit_nbr);

        /**********************
         * WORKER INTERFACES
        *********************/
        //permite atribuir uma task ao Worker
        void attributeTaskService(SubJobW subjobw);

        //promove worker a replica
        void promoteToReplicaService();

        //promove a JobTracker
        void promoteToJobTrackerService();

        /**********************
         * WORKER REPLICA INTERFACE
        **********************/
        //permite atribuir uma task ao Worker
        void informTaskAttributionService();

        /********************
         * Metodos COMUNS
        ********************/    
        //Interface para um novo worker se poder registar num determinado JobTracker
        String registerWorkerService(int id);//tem que responder ao Worker qual o id/ip do Jobtracker que este se deve associar
        
        //permite conhecer o seu id
        int getId();

        //permite conhecer o informação sobre o Worker e permite saber se este está vivo
        WorkerState askNodeInfoService();

        /********************
         * WILL BE MARSHELED BY VALUE
        ********************/
        //permite obter a lista de JT (usado por JT e WR a entrar na rede)
        List<JobTracker> getJTlistService();

        //permite obter a lista de W (usado pelo WR a entrar na rede)
        List<Worker> getWlistService();

        //adiciona um jt da lista (usado pelos JT a entrar na rede)
        void addJTService(JobTracker jt);

        //remove um jt da lista (usado pelos JT a entrar na rede)
        void removeJTService(int id);

        //permite remover um Worker da replica (usado pelos JT quando é detectado que o Worker se desligou)
        void removeWorkerService(int id);

        //permite adicionar um Worker da replica (usado pelos JT quando um novo worker se liga à rede)
        void addWorkerService(Worker w);

        //permite adicionar a replica o conjunto de tasks atribuidos a cada workers
        void addSubJobList(List<SubJobW> subjobList);

        //actualiza o JobTracker do node (Após o WR tomar o lugar o JT)
        void updateJobTracker(JobTracker jt);


    }


    //iMapper
    public interface IMapper {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }

    public interface IMapperTransfer {
        void SendMapperService(SharedClass taskClass);
    }


    //Client Interface
    public interface ClientInterface : IMapperTransfer
    {
        SharedClass provideTask(int taskId, String text_file);

        void deliverTask(IList<KeyValuePair<string, string>> result, int i);

    }


}






   