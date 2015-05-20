using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Reflection;
using System.Text;
using PADI_MapNoReduce;
using System.Threading;
using System.Runtime.Serialization;
using System.Collections;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;


namespace Worker_JobTracker
{
    public class Program
    {

        //if JT == true -> node is jobtracker; if W == true -> node is worker;if JT == true and W = true -> node is Worker serving has a JT backp node
        public bool JT;
        public bool W;
        public bool freeze;//permite para a execução do node
        public bool ready;//permite saber se o worker está livre

        public bool _JT
        {
            get { return JT; }
            set { JT = value; }
        }
        public bool _W
        {
            get { return W; }
            set { W = value; }
        }
        public bool _freeze
        {
            get { return freeze; }
            set { freeze = value; }
        }
        public bool _ready
        {
            get { return ready; }
            set { ready = value; }
        }


       
        public String pupperMasterURL;
        public String entryURL;
        public String serviceURL;
        public JobTracker assignedJobTracker;//JobTracker do nó (apenas de for worker, se nao foir será null)
        public Worker replica;//Worker Replica do JobTracker (apenas se for JT, se nao for será null)
        public int id;//id do proprio no
        public int port;//porto do proprio no
        public String address;//URL do proprio nó
        public int WperJT = 2;//numero máximo de workers por JT da rede 

       
        public String _pupperMasterURL
        {
            get { return pupperMasterURL; }
            set { pupperMasterURL = value; }
        }
        public String _entryURL
        {
            get { return entryURL; }
            set { entryURL = value; }
        }
        public String _serviceURL
        {
            get { return serviceURL; }
            set { serviceURL = value; }
        }
      
        public int _id
        {
            get { return id; }
            set { id = value; }
        }
        public int _port
        {
            get { return port; }
            set { port = value; }
        }
        public String _address
        {
            get { return address; }
            set { address = value; }
        }
        public int _WperJT
        {
            get { return WperJT; }
            set { WperJT = value; }
        }
        
        public List<Worker> workers_list;//apenas usado pelo JobTracker e pelo Worker com funções de JobTracker
        public List<JobTracker> jobtracker_list;//apenas usado pelo JobTracker e pelo Worker com funções de JobTracker
        public SubJobW subJobW;//apenas usado pelo Worker para manter as tarefas por finalizar
        public List<SubJobW> subJobW_list;//Usado pelo JobTracker 

        //timers
        public System.Timers.Timer checkAliveTimer;


        //Construtores
        public Program(int id, String pupperMasterURL, String serviceURL, String entryURL)
        {

            this._pupperMasterURL = pupperMasterURL;
            this._entryURL = entryURL;
            this._serviceURL = serviceURL;    
            this._id = id;
            this.workers_list = new List<Worker>();
            this.jobtracker_list = new List<JobTracker>();

            this.port = 3000 + this.id;
            this.address = createNodeURL(this.port, this.serviceURL);

        }

        /************************
         * INIT METHOD
        *************************/
        public void init(){

            String jobTrackerAddress = entryURL;//para já assumimos que o entryPoint é um JT. Se não for a resposta não serentra no while() 

            //Forma de o nó descobrir a sua função na rede. 
            if (entryURL == null)//Verifica se é o unico na rede
            {
                this.JT = true;
                this.W = false;
                this.assignedJobTracker = null;
                jobTrackerAddress = "";
            }
            else
            {
                               
                //contactamos a primeira vez o serviço de registo no entryNode
                WorkerInterfaceRef jobTrackerServic = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), entryURL);
                String nodeFunction = jobTrackerServic.registerWorkerService(id);
                
                //caso a resposta do nó não seja "JT", "W" ou "WR" o entryNode enviou um URL de outro JT
                while (nodeFunction != "JT" && nodeFunction != "W" && nodeFunction != "WR")
                {
                    jobTrackerAddress = nodeFunction;//Visto que foi returnado um URL guardamos este URL na esperança de ser o JT
                    jobTrackerServic = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), nodeFunction);
                    nodeFunction = jobTrackerServic.registerWorkerService(id);                               
                }

                //contacta o jobTracker para saber o seu id e cria o objecto JobTracker
                if (jobTrackerAddress.Equals("JT"))//se é JT não tem JT associado
                {
                    this.assignedJobTracker = null;
                }else{//Se for worker (replica ou nao) tem que guarda o parametro JT 
                    
                    jobTrackerServic = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), jobTrackerAddress);
                    int jobTrackerId  = jobTrackerServic.getId();
                    this.assignedJobTracker = new JobTracker(jobTrackerAddress, jobTrackerId);

                }


                if (nodeFunction.Equals("JT"))
                {
                    JT = true;
                    W = false;
                }
                else if (nodeFunction.Equals("W"))
                {
                    JT = false;
                    W = true;
                }
                else if (nodeFunction.Equals("WR"))
                {
                    JT = true;
                    W = true;
                }

            }//END: Forma de o nó descobrir a sua função na rede


            /*******************
             *  disponibiliza serviços
            ********************/
            // ------------------------------------------------------------
            BinaryServerFormatterSinkProvider serverProvRef = new BinaryServerFormatterSinkProvider();
            //serverProvRef.TypeFilterLevel = TypeFilterLevel.Full;
            //RemotingConfiguration.CustomErrorsMode = CustomErrorsModes.Off;
            //serverProvRef.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary propBagRef = new Hashtable();
            // -----------------------------------------
            bool isSecure = false;
            propBagRef["port"] = this.port;
            propBagRef["typeFilterLevel"] = TypeFilterLevel.Full;
            propBagRef["name"] = String.Concat("UniqueChannelName", this.id); // here enter unique channel name
            if (isSecure) // if you want remoting comm to be secure and encrypted
            {
                propBagRef["secure"] = isSecure;
                propBagRef["impersonate"] = false; // change to true to do impersonation
            }
            // -----------------------------------------

            //Abre canal para disponibilizar serviços
            System.Console.WriteLine(String.Concat("Id: ",this.id) + " | port: " + this.port + " | address: " + this.address);
            TcpChannel channelRegisterRef = new TcpChannel(propBagRef, null, serverProvRef);
            ChannelServices.RegisterChannel(channelRegisterRef, false);

            //Activa serviço de registo
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(WorkerServicesRef),
                "W",
                WellKnownObjectMode.Singleton);
            WorkerServicesRef.p = this;
            System.Console.WriteLine("Serviço do novo nó está online");
           
           
            /********************************
             * NÓ AJUSTA-SE Á SUA FUNÇÃO (JT, W ou WR)
            ***********************************/
            if (JT == true && W == false)
            {//is JT

                System.Console.WriteLine("Sou um JobTracker");

                //Pede ao JT que lhe atribuiu a função a lista de JT
                if (!jobTrackerAddress.Equals(""))//Verifica se não é o unico na rede
                {
                    WorkerServicesRef getJTlistService = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), assignedJobTracker.address);
                    String[] jobtracker_StringList = getJTlistService.getJTlistService().Split(new string[] { "|" }, StringSplitOptions.None);
                    foreach (String jobtrackerString in jobtracker_StringList)
                    {
                        if (jobtrackerString != "")
                        {
                            this.jobtracker_list.Add(new JobTracker(jobtrackerString));
                        }
                    }
                }

                //Adiciona-se a lista de JT dos outros JT
                JobTracker jt = new JobTracker(address, id);  
                foreach (JobTracker existingJT in this.jobtracker_list)
                {
                    WorkerServicesRef JTService = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), existingJT.address);
                    JTService.addJTService(jt.toString());
                }

                //adiciona-se a si mesmo à sa propria lista.
                jobtracker_list.Add(jt);
          
                System.Console.WriteLine("JobTracker has started fully functional");
            }
            else if (JT == false && W == true)
            {//is Commun Worker 
                
                System.Console.WriteLine("Sou um Worker");
                System.Console.WriteLine("Worker has started fully functional");

            }
            else if (JT == true && W == true)//is Worker that is also JT replica
            {
                System.Console.WriteLine("Sou um Worker com funcoes de replica.");
                
                //Pede lista de JobTrackers e de Workers
                //TODO :  marshallByValue para que cada Replica tenha tudo localmente
                WorkerServicesRef JTServiceReplica = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), assignedJobTracker.address);

                String[] jobtracker_StringList = JTServiceReplica.getJTlistService().Split(new string[] { "|" }, StringSplitOptions.None);
                foreach (String jobtrackerString in jobtracker_StringList)
                {
                    if (jobtrackerString != "")
                    {
                        this.jobtracker_list.Add(new JobTracker(jobtrackerString));
                    }
                    
                }

                String[] workers_StringList = JTServiceReplica.getWlistService().Split(new string[] { "|" }, StringSplitOptions.None);
                foreach (String workerString in workers_StringList)
                {
                    if (workerString != "")
                    {
                        this.workers_list.Add(new Worker(workerString));
                    }
                }   

                System.Console.WriteLine("Worker Replica has started fully functional");

            }
            else
            {
                //WRONG COMBINATION
            }
            //lança TRHEAD para verificar a VIDA dos WORKERS e dos JT
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.checkLifeThread), new object());
        }



        public String createNodeURL(int port, String serviceURL)
        {
            String url = String.Concat("tcp://localhost:",port,"/", serviceURL);
            return url;
        }

        /************************
         * Metodos PARA O JOB TRACKER comunicar com o WORKER
        *************************/
        public void submitSubJob(int split_number, String client_address, string text_file)
        {
            WorkerServicesRef service;

            //Retorna um array que
            List<int> nbrTaskWorkerList = this.splitJobs(this.workers_list.Count(), split_number);//contem o numero de Task que cada Worker vai executar (posição = worker)
            List<int> taskList;
            int i = 0;
            int split_count = 1;
            foreach (int subjobSize in nbrTaskWorkerList)//enviar cada subJob a cada um dos JobTrackers
            {   

                taskList = new List<int>();
                //cria a numeração das tasks para cada worker
                for (int j = 0; j < subjobSize; j++)
                {
                    taskList.Add(split_count);
                    split_count++;
                }

                //cria subJobW para guardar na list de subJobW do JT e para enviar ao Worker
                subJobW_list.Add( new SubJobW(this.workers_list[i].id, this.id, client_address, text_file, taskList) );

                //Lança Thread para enviar os SubJobs aos workers
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.submitSubJobThread), new SubJobArguments(this.workers_list[i].id, this.id, this.workers_list[i].address, client_address, text_file, taskList));

                //incrementa contador
                i++;
            }   

            //Envia SubJob para replica
            foreach (Worker w in workers_list)
            {
                if (w.replica == true)//procura pela replica
                {   
                    String jb_listString = "";
                    int counter = 1;
                    foreach (SubJobW sjw in this.subJobW_list)
                    {
                        if (counter == this.subJobW_list.Count)
                        {
                            jb_listString = "" + jb_listString + sjw.toString();
                        }
                        else
                        {
                            jb_listString = "" + jb_listString + sjw.toString() + "|";
                        }
                        counter++;
                    }

                    service = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), w.address);
                    service.addSubJobList(jb_listString);
                }                
            }
        }

        public void replaceWorker(int old_workerId, int new_workerId)
        {
            //Hipotese1 pode passar por mudar o id e o timestamp do worker do subJobW que está lento. Como o metedo é invocado como referencia remota da proxima vez que o node pedir uma task já não tem lá nada para ele
            SubJobW oldWorker_subjob = this.subJobW_list.Find(x => x.workerId == old_workerId);
            SubJobW newWorker_subjob = this.subJobW_list.Find(x => x.workerId == new_workerId);
            
            oldWorker_subjob.workerId = new_workerId;

            newWorker_subjob.starting_unixTimeStamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds; ;
            newWorker_subjob.workerId = old_workerId;
            newWorker_subjob.initial_task_nbr = newWorker_subjob.taskList.Count;
               
        }

        /************************
        * METODOS PARA O WORKER
        ************************/
        //comunica com a aplicacao cliente a pedir o trabalho que lhe foi atribuido pelo Job Tracker
        private String getTask(int taskNumber)
        {
            String task_string = "";
            return task_string;
        }

        //comunica com a aplicacao cliente a enviar o trabalho que lhe foi atribuido pelo Job Tracker
        private void sendTask(String job_output)
        {
            //this.subJobW.jobTrackerId
        }

        

        //executa trabalho
        private void execute_task(int taskId)
        {
            
            String split_string = getTask(taskId);

            //codigo para tratar o split

            String split_output = "";
            sendTask(split_output);

            //retira task da sua lista local
            this.subJobW.taskList.Remove(taskId);
  
        }

        //Executa um conunto de tasks
        public void executeSubJob()
        {
            this.ready = false;
            foreach (int taskId in this.subJobW.taskList)
            {
                execute_task(taskId);
            }
            this.ready = true;

        }


        public void promoteToReplica()
        {
            this.JT = true;
            //pede lista de workers da rede ao novo JobTracker (ja esta actalizado)
            WorkerServicesRef JTService = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), this.jobtracker_list[0].address);
            
            this.jobtracker_list = null;
            String[] jobtrackerStringList = JTService.getJTlistService().Split(new string[] { "|" }, StringSplitOptions.None);
            foreach (String jobtrackerString in jobtrackerStringList)
            {
                if (jobtrackerString != "")
                {
                    this.jobtracker_list.Add(new JobTracker(jobtrackerString));
                }
            }

            this.workers_list = null;
            String[] workers_StringList = JTService.getWlistService().Split(new string[] { "|" }, StringSplitOptions.None);
            foreach (String workerString in workers_StringList)
            {
                if (workerString != "")
                {
                    this.workers_list.Add(new Worker(workerString));
                }
            }
        }

        public void promoteToJobTracker()
        {
            this.W = false;//altera o estado do node

            //pede uma nova lista de JobTrackers a um JT arbitrario da sua lista
            WorkerServicesRef JTService = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), this.jobtracker_list[0].address);
            this.jobtracker_list = null;
            String[] jobtrackerStringList = JTService.getJTlistService().Split(new string[] { "|" }, StringSplitOptions.None);
            foreach (String jobtrackerString in jobtrackerStringList)
            {
                if (jobtrackerString != "")
                {
                    this.jobtracker_list.Add(new JobTracker(jobtrackerString));
                }
            }

            //Adiciona-se a lista de JT dos outros JT e remove o anterior
            JobTracker jtPromoted = new JobTracker(this.address, this.id);
            foreach (JobTracker existingJT in this.jobtracker_list)
            {
                WorkerServicesRef JTService2 = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), existingJT.address);
                JTService2.addJTService(jtPromoted.toString());
                JTService2.removeJTService(this.assignedJobTracker.id);
            }

            //remove-se a si localmente e adiciona o outro
            this.jobtracker_list.Add(jtPromoted);
            this.jobtracker_list.RemoveAll(x => x.id == this.assignedJobTracker.id);

            //Avisa todos os Workers que o novo JobTracker é ele
            foreach (Worker existingW in this.workers_list)
            {
                WorkerServicesRef JTService3 = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), existingW.address);
                JTService3.updateJobTracker(jtPromoted.toString());
            }

            //Promove o primeiro Worker da lista a Replica
            WorkerServicesRef JTServiceReplica = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), this.workers_list[0].address);
            JTServiceReplica.promoteToReplicaService();

            //coloca a variavel que guarda o jobTracker a null 
            this.assignedJobTracker = null;

            System.Console.WriteLine("Serviço do JobTracker está online");
        }

        //TODO : Rever repartição de Splits... acho que todos os JT começam a contagem do Split 1.... deviam ser todos dif...
        //Retorna um array de inteiro com a mm dimensão do numero de w nos com o numero de tarefas/tasks/splits que cada no fica
        public List<int> splitJobs(int nbr_nodes, int nbr_splits)
        {

            List<int> splits = new List<int>();

            float nodesPerSplit = (float)nbr_splits / (float)nbr_nodes;
            int nodesPerSplitTruncated = (int) Math.Floor(nodesPerSplit);
            
            if (nodesPerSplit < 1)
            {
                System.Console.WriteLine("nodesPerSplit < 1");
                //Se o numero de Nodes é maior que o numero de splits basta atribuir um split a cada node e alguns ficam a sobrar
                for (int i = 0; i < nbr_splits; i++)
                {
                    splits.Add(1);
                }
            }
            else
            {
                System.Console.WriteLine("nodesPerSplit > 1");
                float accumFloat = 0;
                int accumInt = 0;
                int workersWithJob = 0;
                while (accumFloat < nbr_splits)
                {
                   // System.Console.WriteLine(String.Concat("accumFloat -> ", accumFloat));
                    if ( ((nbr_splits - accumInt) < nodesPerSplitTruncated) || (workersWithJob + 1 == nbr_nodes) )//Ja so falta o ultimo worker e o numero de tasks que sobram e menor do que era suposto
                    {
                        splits.Add(nbr_splits - accumInt);
                        accumInt += nbr_splits - accumInt;
                        accumFloat += nodesPerSplit;
                        break;
                    }
                    else
                    {
                        if( (accumFloat - accumInt) >= 1){ //se a dif entre os acumuladores é superior a um o proximo Worker tem que receber mais um JOB que os outros para equilibrar

                            accumInt += nodesPerSplitTruncated + 1;
                            splits.Add(nodesPerSplitTruncated + 1);
                        }
                        else
                        {
                            accumInt += nodesPerSplitTruncated;//soma acumulador real (truncado)
                            splits.Add(nodesPerSplitTruncated);
                        }
                        accumFloat += nodesPerSplit;//somo acumulador ideal (se se podessem dividir tarefas em fracções)                        

                    }
                    workersWithJob++;
                }
            }


            return splits;

        }

        /**************************
         * THREADS
        ***************************/
        public void checkLifeThread(object arg)
        {
            Console.WriteLine("checkLifeThread starting...");
            WorkerInterfaceRef service;
            while (true)
            {
                if (this.freeze == false && this.W == true && this.JT == true)//JT replica node (is also worker)
                {
                    Console.WriteLine(assignedJobTracker.address);
                    service = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), assignedJobTracker.address);
                    WorkerState jobTrackerState = service.askNodeInfoService();

                    //JobTracker que não responde!
                    if (jobTrackerState == null)
                    {
                        this.promoteToJobTracker();//auto promove-se a REPLICA
                        if (workers_list.Count > 0)
                        {
                            //pode ser PassedBrReferende e não ByValue
                            service = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), this.workers_list[0].address);
                            service.promoteToReplicaService();
                        }
                    }
                    Console.WriteLine("Thread checkJobTrackerLife running");
                }
                else if (this.freeze == false && this.W == false && this.JT == true)//JB node
                {
                    int total_workers_time = 0;
                    int total_completed_tasks = 0;
                    double average_time_to_complete_task;
                    //asks for a state message to every workers
                    foreach (Worker w in this.workers_list)
                    {
                        //pode ser PassedBrReferende e não ByValue
                        service = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), w.address);
                        WorkerState stateWorker = service.askNodeInfoService();

                        SubJobW subjob = this.subJobW_list.Find(x => x.workerId == w.id);

                        //Worker que não responde!
                        if (stateWorker == null)
                        {
                            this.workers_list.RemoveAll(x => x.id == w.id);//apaga worker da sua lista de Workers

                            if (w.replica == true && workers_list.Count > 0)//O worker que nao respondeu era o seu replica. Necessário promover um worker a replica
                            {
                                WorkerInterfaceRef promotionService = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), workers_list[0].address);
                                promotionService.promoteToReplicaService();
                            }
                            else if (w.replica == false)//O worker que nao responde não era replica. Necessário informar a replica da ausencia deste Worker
                            {
                                foreach (Worker existingW in this.workers_list)//procura pela replica
                                {
                                    if (existingW.replica == true)
                                    {
                                        WorkerServicesRef WorkerService = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), existingW.address);
                                        WorkerService.removeWorkerService(existingW.id);
                                    }
                                }

                            }

                        }
                        else//Se o worker responde vamos verificar o seu desempenho
                        {
                            int tasks_exected = subjob.initial_task_nbr - stateWorker.tasks_remaining;//calcula numero de tarefas executadas até agora
                            int elapsed_time = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds - this.subJobW.starting_unixTimeStamp;//calcula o tempo que decorreu entre
                            w.timePerTask = elapsed_time / tasks_exected;
                            total_workers_time += elapsed_time;
                            total_completed_tasks += tasks_exected;
                        }

                    }

                    if (total_completed_tasks > 0)
                    {
                        average_time_to_complete_task = total_workers_time / total_completed_tasks;
                    }
                    else
                    {
                        average_time_to_complete_task = 9999999999;
                    }
                    
                    foreach (Worker w in this.workers_list)
                    {
                        //verifica se o worker demora pelo menos o dobro do tempo que a média dos workers.
                        if( (2 * average_time_to_complete_task)  < w.timePerTask ){

                            foreach (Worker freeWorker in this.workers_list)
                            {
                                if (freeWorker.ready == true)
                                {
                                    //TODO: Code to replace worker
                                    this.replaceWorker(w.id, freeWorker.id);
                                }
                            }
                        }
                           
                    }


                    Console.WriteLine("Thread checkWorkerLife running");
                }//if end
                System.Threading.Thread.Sleep(5000);//espera 5seg
            }
        }


        //SubmitJob Threads
        public void submitJobThread(Object arg)
        {
            JobArguments job = (JobArguments)arg;
            WorkerServicesRef service = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), job.address);
            service.submitSubJobService(job.nbr_splits, job.clientAddress, job.text_file);

        }

        //SubmitJob Threads
        public void submitSubJobThread(Object arg)
        {
            SubJobArguments subjob = (SubJobArguments)arg;

            //atribui a task ao worker
            WorkerServicesRef service = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), subjob.address);
            service.attributeTaskService(new SubJobW(subjob.workerId, subjob.jobTrackerId, subjob.clientAddress, subjob.text_file, subjob.task_list));

        }


        /**************************
         * MAIN
        ***************************/
        static void Main(string[] args)
        {
            String pupperMasterURL = "tcp://localhost:1000/W";
            String entryURL = null;
            String serviceURL = "W";
            
            Program JT1 = new Program(1, pupperMasterURL, serviceURL, entryURL);
            JT1.init();
            
            System.Console.ReadLine();
            Program W1 = new Program(2, pupperMasterURL, serviceURL, JT1.address);
            W1.init();
            Console.WriteLine("main: W1 as started");
            Console.WriteLine(W1.assignedJobTracker.address);
            System.Console.ReadLine();
        }
    }




    /********************************
        * Services
    *********************************/
//     //Serviços disponibilizadios Pelo no
//    [Serializable]
//    internal class WorkerServicesRef : ISerializable, WorkerInterfaceVal
//    {
//
//        public static Program p;
//
//        //permite que um JT obtenha a lista de JT existentes
//        public List<JobTracker> getJTlistService()
//        {
//            return p.jobtracker_list;
//        }
//
//        //permite obter a lista de W
//        public List<Worker> getWlistService()
//        {
//            return p.workers_list;
//        }
//
//        //permite que seja adicionao um novo JT á lista
//        public void addJTService(JobTracker jt)
//        {
//            p.jobtracker_list.Add(jt);
//        }
//
//        //Remove JobTracker à sua lista
//        public void removeJTService(int id)
//        {
//            p.jobtracker_list.RemoveAll(x => x.id == id);
//        }
//
//
//        //Remove worker da sua lista
//        public void removeWorkerService(int id)
//        {
//            p.workers_list.RemoveAll(x => x.id == id);
//        }
//
//        //Adiciona worker à sua lista
//        public void addWorkerService(Worker w)
//        {
//            p.workers_list.Add(w);
//        }
//
//        //permite conhecer o informação sobre o Worker e permite saber se este está vivo
//        public void addSubJobList(List<SubJobW> subjobList)
//        {
//            p.subJobW_list = subjobList;
//        }
//
//        //actualiza o jobTracker
//        public void updateJobTracker(JobTracker jt)
//        {
//            p.assignedJobTracker = jt;
//        }
//
//    }
    //Serviços disponibilizadios Pelo no
    internal class WorkerServicesRef: MarshalByRefObject, WorkerInterfaceRef
    {

        public static Program p;

        /***************************
         * JOBTRACKER SERVICES
         * ************************/
        //Permite que o cliente submeta um novo Job
        public String submitJobService(int split_number, String client_address, string text_file)
        {
            String response = "";
            if (p.W == true)//O nó para o qual foi enviado o JOB não e jobTracker
            {
                //responde com o address do JT desigando para este worker
                response = p.assignedJobTracker.address;
            }
            else if (p.W == false && p.JT == true)//O nó é um job Tracker
            {

                List<int> subJobSizeList = p.splitJobs(p.jobtracker_list.Count(), split_number);//Numero das task para cada no (lista de igual dimensão)
                
                int i = 0; 
                foreach (int subjobSize in subJobSizeList)//enviar cada subJob a cada um dos JobTrackers
                {
                    if (p.jobtracker_list[i].id == p.id)//Se é o proprio
                    {
                        p.submitSubJob(subjobSize, client_address, text_file);
                    }
                    else { //Se é outro
                        //SERVICES
                        //lança TRHEAD para enviar o Job aos JT
                        ThreadPool.QueueUserWorkItem(new WaitCallback(p.submitJobThread), new JobArguments(subjobSize, p.jobtracker_list[i].address, client_address, text_file));
                        
                    }
                    i++;
                }
                response = "ok";
            }

           
            return response;//string com o address do JT o com "ok" para o cliente perceber que fo pedido esta a ser executado
        }

        //Permite que o Job seja dividido por vários JobTrackers
        public void submitSubJobService(int split_number, String client_address, string text_file)
        {
            p.submitSubJob(split_number, client_address, text_file);
        }


        

        public int getId()
        {
            return p.id;
        }
        /***************************
         * WORKER SERVICES
         * ************************/
        //Serviço para o CLiente conseguir enviar ao Worker a TASK!
        public bool SendMapperService(byte[] code, string className, int splitNumber, String clientURL, String file )
        {
            Assembly assembly = Assembly.Load(code);
            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + className))
                    {
                        // create an instance of the object
                        object ClassObj = Activator.CreateInstance(type);

                        // Dynamically Invoke the method
                        object[] args = new object[] { "testValue" };
                        object resultObject = type.InvokeMember("Map",
                            BindingFlags.Default | BindingFlags.InvokeMethod,
                                null,
                                ClassObj,
                                args);
                        IList<KeyValuePair<string, string>> result = (IList<KeyValuePair<string, string>>)resultObject;
                        Console.WriteLine("Map call result was: ");
                        foreach (KeyValuePair<string, string> p in result)
                        {
                            Console.WriteLine("key: " + p.Key + ", value: " + p.Value);
                        }
                        return true;
                    }
                }
            }
            throw (new System.Exception("could not invoke method"));
            //          return true;
        }

        //permite atribuir uma task ao Worker
        public void attributeTaskService(SubJobW subjobw)
        {
            p.subJobW = subjobw;
            p.executeSubJob();

        }

        //permite conhecer o informação sobre o Worker e permite saber se este está vivo
        public WorkerState askNodeInfoService() 
        {
            WorkerState ws;
            if (p.JT == true && p.W == false)//JobTracker
            {
                ws = new WorkerState(p.ready, p.freeze, p.subJobW.taskList.Count);
                return ws;
            }
            else
            {
                ws = new WorkerState(p.ready, p.freeze,0);
            }
            return ws;
        }

        public void promoteToReplicaService()
        {
            p.promoteToReplica();
           
        }

        public void promoteToJobTrackerService()
        {
            p.promoteToJobTracker();
        }

        /***************************
         * WORKER REPLICA SERVICES
         * ************************/
        //permite atribuir uma task ao Worker
        public void informTaskAttributionService()
        {

        }

        

        /***************************
        * Metodos COMUNS
        * ************************/
        //Interface para um novo worker se poder registar num determinado JobTracker
        public String registerWorkerService(int id)//tem que responder ao Worker qual o id/ip do Jobtracker que este se deve associar
        {
            String workerFunction;

            if (p.W == true)
            {//ou é Worker simples ou replica
                workerFunction = p.assignedJobTracker.address;
            }
            else
            {
                if (p.WperJT == p.workers_list.Count)//JT cheio. Novo nó terá que ser JT
                {
                    workerFunction = "JT";
                    foreach (JobTracker jobtrackerInList in p.jobtracker_list)
                    {

                        Console.WriteLine("job tracker id and address", jobtrackerInList.id, jobtrackerInList.address);
                        if (jobtrackerInList.nbr_worker < p.WperJT)//verifica se existe algm JobTracker com vagas
                        {
                            workerFunction = jobtrackerInList.address;//Se existe algum worker na lista livre o JT encaminha o nó para lá
                            break;
                        }
                    }

                }
                else if (p.workers_list.Count == 0)//JT sem nenhum Worker.
                {
                    workerFunction = "WR";
                    Worker w = new Worker(p.createNodeURL(id, p.serviceURL), id, true);
                    p.workers_list.Add(w);
                }
                else
                {
                    workerFunction = "W";
                    Worker w = new Worker(String.Concat("tcp://localhost:300", id), id, false);
                    p.workers_list.Add(w);//adiciona worker à sua lista de workers

                    //Procura na sua lista de Workers qual a replica e envia a informação do novo worker
                    foreach (Worker workerInList in p.workers_list)
                    {

                        Console.WriteLine("Worker id and address", workerInList.id, workerInList.address);
                        if (workerInList.replica == true)
                        {
                            WorkerServicesRef workerReplicaService = (WorkerServicesRef)Activator.GetObject(typeof(WorkerServicesRef), p.assignedJobTracker.address);
                            workerReplicaService.addWorkerService(workerInList.toString());
                        }
                    }
                }
            }
            return workerFunction;
        }


        /***********************
         * MARSHALL BY VALUE
         * ************************/
        //permite que um JT obtenha a lista de JT existentes
        public String getJTlistService()
        {
            String jt_list_string = "";
            int counter = 1;
            foreach (JobTracker jt in p.jobtracker_list)
            {

                if (counter == p.jobtracker_list.Count)
                {
                    jt_list_string = "" + jt_list_string + jt.toString();
                }
                else
                {
                    jt_list_string = "" + jt_list_string + jt.toString() + "|";
                }
                counter++;
            }
            return jt_list_string;
        }
 
        //permite obter a lista de W
        public String getWlistService()
        {
            String w_list_string = "";
            int counter = 1;
            foreach (Worker w in p.workers_list)
            {
                if (counter == p.jobtracker_list.Count)
                {
                    w_list_string = "" + w_list_string + w.toString();
                }
                else
                {
                    w_list_string = "" + w_list_string + w.toString() + "|";
                }
                counter++;
            }
            return w_list_string;
        }
 
        //permite que seja adicionao um novo JT á lista
        public void addJTService(String jt)
        {

            p.jobtracker_list.Add(new JobTracker(jt));
        }
 
        //Remove JobTracker à sua lista
        public void removeJTService(int id)
        {
            p.jobtracker_list.RemoveAll(x => x.id == id);
        }
 
 
        //Remove worker da sua lista
        public void removeWorkerService(int id)
        {
            p.workers_list.RemoveAll(x => x.id == id);
        }
 
        //Adiciona worker à sua lista
        public void addWorkerService(String wString)
        {
            p.workers_list.Add(new Worker(wString));
        }
 
        //permite conhecer o informação sobre o Worker e permite saber se este está vivo
        public void addSubJobList(String subjobListString)
        {

            string[] subJobStringList = subjobListString.Split(new string[] { "|" }, StringSplitOptions.None);
            foreach (String subJobString in subJobStringList)
            {
                if (subJobString != "")
                {
                    p.subJobW_list.Add(new SubJobW(subJobString));
                }
                
            }
            
        }
 
        //actualiza o jobTracker
        public void updateJobTracker(String jtString)
        {
            p.assignedJobTracker = new JobTracker(jtString);
        }

    }
}
