using System;
using System.Collections.Generic;
using System.IO;

/// CHANGE TO CLIENT: Renato


namespace PADI_MapNoReduce
{
  
    /************************
     * Classes
    **************************/
 
    public class Worker
    {
        public double rank;//classificação de fiabilidade (contactados primeiro)
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

    }

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

    public class WorkerState
    {
        public bool ready;//verifica se o nó nestá livre ou está a trabalhar
        public bool freeze;//permite para a execução do node
        public SubJobW subJobW;

        public WorkerState(bool ready, bool freeze, SubJobW subJobW)
        {
            this.ready = ready;
            this.freeze = freeze;
            this.subJobW = subJobW;
        }
    }

    public class SubJobW
    {
     
        public List<int> taskList;
        public int workerId;
        public int jobTrackerId;
        public String clientAddress;
        public String text_file;

        public SubJobW(int workerId, int jobTrackerId, String clientAddress, String text_file, List<int> taskList)
        {
            this.taskList = taskList;
            this.workerId = workerId;
            this.jobTrackerId = jobTrackerId;
            this.clientAddress = clientAddress;
            this.text_file = text_file;
        }

    }
        

    //Worker + Replica Interface
    public interface WorkerInterface : IMapperTransfer
    {

        /**********************
         * JOB TRACKER INTERFACE
        **********************/
        //Permite que o cliente submeta um novo Job
        String submitJobService(int split_number);
        //Permite que o Job seja dividido por vários JobTrackers
        void submitSubJobService(int split_number);

        //permite obter a lista de JT
        List<JobTracker> getJTlistService();

        //permite obter a lista de W
        List<Worker> getWlistService();


        //adiciona um jt da lista
        void addJTService(JobTracker jt);

        //remoce um jt da lista
        void removeJTService(int id);

        //permite conhecer o seu id
        int getId();

        /**********************
         * WORKER INTERFACES
        *********************/
        //permite atribuir uma task ao Worker
        void attributeTaskService(SubJobW subjobw);

        //promove worker a replica
        void promoteToReplicaService();

        //promove a JobTracker
        void promoteToJobTrackerService();

        //actualiza o JobTracker do node
        void updateJobTracker(JobTracker jt);

        /**********************
         * WORKER REPLICA INTERFACE
        **********************/
        //permite atribuir uma task ao Worker
        void informTaskAttributionService();

        //permite remover um Worker da replica
        void removeWorkerService(int id);

        //permite adicionar um Worker da replica
        void addWorkerService(Worker w);
        
        //permite adicionar a replica o conjunto de tasks atribuidos a cada workers
        void addSubJobList(List<SubJobW> subjobList);

        /********************
         * Metodos COMUNS
        ********************/
        //permite conhecer o informação sobre o Worker e permite saber se este está vivo
        WorkerState askNodeInfoService();

        //Interface para um novo worker se poder registar num determinado JobTracker
        String registerWorkerService(int id);//tem que responder ao Worker qual o id/ip do Jobtracker que este se deve associar
    
    }


    //iMapper
    public interface IMapper {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }

    public interface IMapperTransfer {
        bool SendMapperService(byte[] code, string className, int splitNumber, String clientURL, String file);
    }
}







    /*
	public class MyRemoteObject : MarshalByRefObject  {


        // LOCAL METHOD INIT
        public void INIT()
        {

        }

        // LOCAL METHOD SUBMIT
        public void SUBMIT(string filepath, int nTasks, string outputPath, IMap mapImplementation)
        {
          // UserApplication (and PuppetMaster, for testing) call this method in order to send new mapping jobs to Server(Workers)
        }


        // REMOTE Provide Tasks
        public void ProvideTasks(string textbegin, string textEnd)
        {
          // Server(Workers) call this method in order to receive the Task they are supposed to work on
        }

        // REMOTE Receive Output
        public void ReceiveOutput(string processedTask)
        {
          // Server(Workers) call this method in order to send the client the result of processing the Tasks
        }
  }
     * */
