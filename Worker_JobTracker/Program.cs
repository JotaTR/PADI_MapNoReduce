﻿using System;
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
        public int WperJT = 0;//numero máximo de workers por JT da rede 


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
            this.subJobW_list = new List<SubJobW>();
            this.port = 30000 + this.id;
            this.address = serviceURL;
            this.freeze = false;
            this.ready = true;
            this.init();
        }

        /************************
         * INIT METHOD
        *************************/
        public void init()
        {

            String jobTrackerAddress = entryURL;//para já assumimos que o entryPoint é um JT. Se não for a resposta não serentra no while() 

            //Forma de o nó descobrir a sua função na rede. 
            if (entryURL == "")//Verifica se é o unico na rede
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
                String nodeFunction = jobTrackerServic.registerWorkerService(this.id, this.address);

                //caso a resposta do nó não seja "JT", "W" ou "WR" o entryNode enviou um URL de outro JT
                while (true)
                {
                    if (nodeFunction == "JT" || nodeFunction == "W" || nodeFunction == "WR")
                    {
                        break;
                    }
                    System.Console.WriteLine("Enter the while");
                    System.Console.WriteLine(nodeFunction);
                    jobTrackerAddress = nodeFunction;//Visto que foi returnado um URL guardamos este URL na esperança de ser o JT
                    jobTrackerServic = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), nodeFunction);
                    nodeFunction = jobTrackerServic.registerWorkerService(this.id, this.address);
                    System.Console.WriteLine("exiting the while");
                    System.Console.WriteLine(nodeFunction);

                }

                //contacta o jobTracker para saber o seu id e cria o objecto JobTracker
                if (jobTrackerAddress.Equals("JT"))//se é JT não tem JT associado
                {
                    this.assignedJobTracker = null;
                }
                else
                {//Se for worker (replica ou nao) tem que guarda o parametro JT 

                    jobTrackerServic = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), jobTrackerAddress);
                    int jobTrackerId = jobTrackerServic.getId();
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
            propBagRef["name"] = String.Concat("UniqueChannelName3000", this.id); // here enter unique channel name
            if (isSecure) // if you want remoting comm to be secure and encrypted
            {
                propBagRef["secure"] = isSecure;
                propBagRef["impersonate"] = false; // change to true to do impersonation
            }
            // -----------------------------------------

            //Abre canal para disponibilizar serviços
            System.Console.WriteLine(String.Concat("Id: ", this.id) + " | port: " + this.port + " | address: " + this.address);
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
                    WorkerInterfaceRef getJTlistService = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), assignedJobTracker.address);
                    jobtracker_list = getJTlistService.getJTlistService();

                }

                //Adiciona-se a lista de JT dos outros JT
                JobTracker jt = new JobTracker(address, id);
                foreach (JobTracker existingJT in this.jobtracker_list)
                {
                    WorkerInterfaceRef JTService = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), existingJT.address);
                    JTService.addJTService(jt);
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
                WorkerInterfaceRef JTServiceReplica = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), assignedJobTracker.address);
                this.jobtracker_list = JTServiceReplica.getJTlistService();//recebe lista de JT
                this.workers_list = JTServiceReplica.getWlistService();//recebe lista de W do HT

                System.Console.WriteLine("Worker Replica has started fully functional");
                System.Console.WriteLine("Worker has started fully functional");

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
            //String url = String.Concat("tcp://localhost:",port,"/", serviceURL);
            return serviceURL;
        }

        /************************
         * Metodos PARA O JOB TRACKER comunicar com o WORKER
        *************************/
        public void submitSubJob(int split_number, String client_address, string text_file, int startingSplit_nbr)
        {
            WorkerInterfaceRef service;

            //Retorna um array que
            List<int> nbrTaskWorkerList = this.splitJobs(this.workers_list.Count(), split_number);//contem o numero de Task que cada Worker vai executar (posição = worker)
            List<int> taskList;
            int i = 0;
            foreach (int subjobSize in nbrTaskWorkerList)//enviar cada subJob a cada um dos JobTrackers
            {

                taskList = new List<int>();
                //cria a numeração das tasks para cada worker
                for (int j = 0; j < subjobSize; j++)
                {
                    taskList.Add(startingSplit_nbr);
                    startingSplit_nbr++;
                }

                //cria subJobW para guardar na list de subJobW do JT e para enviar ao Worker
                subJobW_list.Add(new SubJobW(this.workers_list[i].id, this.id, client_address, text_file, taskList));

                //Lança Thread para enviar os SubJobs aos workersa
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.executeSubJobThread), new SubJobArguments(this.workers_list[i].id, this.id, this.workers_list[i].address, client_address, text_file, taskList));

                //incrementa contador
                i++;
            }

            //Envia SubJob para replica
            foreach (Worker w in workers_list)
            {
                if (w.replica == true)//procura pela replica
                {
                    service = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), w.address);
                    service.addSubJobList(subJobW_list);
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
        private void execute_task(int taskId, String text_file, ClientInterface service)
        {

            String split_string = getTask(taskId);

            //Contacta cliente e pede Split
            SharedClass taskClass = service.provideTask(taskId, text_file);

            //Processa a task
            sendMapper(taskClass);
            service.deliverTask(sendMapper(taskClass), taskId);

            //retira task da sua lista local
            this.subJobW.taskList.Remove(taskId);

        }



        public void promoteToReplica()
        {
            this.JT = true;
            //pede lista de workers da rede ao novo JobTracker (ja esta actalizado)
            WorkerInterfaceRef JTService = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), this.assignedJobTracker.address);
            this.jobtracker_list = JTService.getJTlistService();
            this.workers_list = JTService.getWlistService();
        }

        public void promoteToJobTracker()
        {
            this.W = false;//altera o estado do node

            //Cria-se como JT e remove o anterior
            JobTracker jtPromoted = new JobTracker(this.address, this.id);

            //Remove-se da lista de workers 
            this.workers_list.RemoveAll(x => x.id == this.id);

            this.jobtracker_list.RemoveAll(x => x.id == this.assignedJobTracker.id);
            //pede uma nova lista de JobTrackers a um JT arbitrario da sua lista
            if (jobtracker_list.Count > 0)
            {
                WorkerInterfaceRef JTService = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), this.jobtracker_list[0].address);
                this.jobtracker_list = JTService.getJTlistService();
                //Adiciona-se a lista de JT dos outros JT e remove o anterior
                foreach (JobTracker existingJT in this.jobtracker_list)
                {
                    WorkerInterfaceRef JTService2 = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), existingJT.address);
                    JTService2.addJTService(jtPromoted);
                    JTService2.removeJTService(this.assignedJobTracker.id);
                }
            }
            //Adiciona-se a si localmente 
            this.jobtracker_list.Add(jtPromoted);

            if (this.workers_list.Count > 0)
            {
                //Avisa todos os Workers que o novo JobTracker é ele
                List<int> workersToRemove = new List<int>();
                foreach (Worker existingW in this.workers_list)
                {
                    WorkerInterfaceRef JTService3 = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), existingW.address);
                    try
                    {
                        JTService3.updateJobTracker(jtPromoted);
                    }
                    catch (Exception e)
                    {
                        workersToRemove.Add(existingW.id);
                    }
                    
                }
                foreach (int wToRemove in workersToRemove)
                {
                    this.workers_list.RemoveAll(x => x.id == wToRemove);
                }
                
                //Promove o primeiro Worker da lista a Replica
                WorkerInterfaceRef JTServiceReplica = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), this.workers_list[0].address);
                try
                {
                    JTServiceReplica.promoteToReplicaService();
                }
                catch (Exception e){}
            }
            //coloca a variavel que guarda o jobTracker a null 
            this.assignedJobTracker = null;

            System.Console.WriteLine("Serviço do JobTracker está online");
        }

        //TODO : Rever repartição de Splits... acho que todos os JT começam a contagem do Split 1.... deviam ser todos dif...
        //Retorna um array de inteiro com a mm dimensão do numero de w nos com o numero de tarefas/tasks/splits que cada no fica
        public List<int> splitJobs(int nbr_nodes, int nbr_splits)
        {

            List<int> splits = new List<int>();
            for (int i = 0; i < nbr_nodes; i++)
            {
                splits[i] = 0;
            }

            int j = 0;
            for (int i = 0; i < nbr_splits; i++)
            {
                splits[j]++;
                if (j == (nbr_nodes - 1))
                {
                    j = 0;
                }
                else
                {
                    j++;
                }
            }

            return splits;

        }


        //Serviço para o CLiente conseguir enviar ao Worker a TASK!
        public IList<KeyValuePair<string, string>> sendMapper(SharedClass taskClass)
        {
            Assembly assembly = Assembly.Load(taskClass.code);
            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + taskClass.className))
                    {
                        // create an instance of the object
                        object ClassObj = Activator.CreateInstance(type);

                        // Dynamically Invoke the method
                        object[] args = new object[] { taskClass.split };
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
                        return result;
                    }
                }
            }
            throw (new System.Exception("could not invoke method"));
            //          return true;
        }
        /**************************
          * THREADS
         ***************************/
        public void checkLifeThread(object arg)
        {
            Console.WriteLine("checkLifeThread starting...");
            Console.WriteLine(this.id);
            WorkerInterfaceRef service;
            WorkerState jobTrackerState;
            WorkerState stateWorker;
            WorkerInterfaceRef promotionService;
            WorkerInterfaceRef WorkerService;
            while (true)
            {

                if (this.freeze == false && this.W == true && this.JT == true)//JT replica node (is also worker)
                {
                    service = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), assignedJobTracker.address);
                    try
                    {
                        jobTrackerState = service.askNodeInfoService();
                    }
                    catch (Exception e)
                    {
                        this.promoteToJobTracker();//auto promove-se a REPLICA
                        if (workers_list.Count > 1)
                        {
                            //Contacta worker a promover-lo a replica
                            service = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), this.workers_list[0].address);
                            try
                            {
                                service.promoteToReplicaService();
                            }
                            catch (Exception e8) { }
                            
                        }

                    }
                    Console.WriteLine("Thread checkJobTrackerLife running");
                }
                else if (this.freeze == false && this.W == false && this.JT == true)//JT node
                {
                    int total_workers_time = 0;
                    int total_completed_tasks = 0;
                    double average_time_to_complete_task;
                    //asks for a state message to every workers
                    List<int> workersToRemove = new List<int>();
                    foreach (Worker w in this.workers_list)
                    {

                        //Pede estado do worker ao worker
                        service = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), w.address);
                        try
                        {
                            stateWorker = service.askNodeInfoService();
                            if (subJobW_list.Count > 0)
                            {
                                SubJobW subjob = this.subJobW_list.Find(x => x.workerId == w.id);
                                int tasks_exected = subjob.initial_task_nbr - stateWorker.tasks_remaining;//calcula numero de tarefas executadas até agora
                                int elapsed_time = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds - this.subJobW.starting_unixTimeStamp;//calcula o tempo que decorreu entre
                                w.timePerTask = elapsed_time / tasks_exected;
                                total_workers_time += elapsed_time;
                                total_completed_tasks += tasks_exected;
                            }
                        }
                        catch (Exception e)
                        {
                            //Worker que não responde!
                            workersToRemove.Add(w.id);
                            

                            if (w.replica == true && workers_list.Count > 1)//O worker que nao respondeu era o seu replica. Necessário promover um worker a replica
                            {
                                promotionService = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), workers_list[1].address);
                                promotionService.promoteToReplicaService();
                                workers_list[1].replica = true;

                            }else{
                            }
                            if (this.subJobW_list.Find(x => x.workerId == w.id) != null)//temos que verificar se o worker tem Task assigned...
                            {

                            }

                        }

                    }

                    //Retira workers com mau funcionamento da sua propria lista (ja informou o Replica se algum W normal falhou)
                    foreach (int workerId in workersToRemove)
                    {
                        this.workers_list.RemoveAll(x => x.id == workerId);//apaga worker da sua lista de Workers
                    }

                    //Remove o worker que falhou da lista da replica (nova ou antiga)
                    foreach (Worker existingW in this.workers_list)//procura pela replica
                    {
                        if (existingW.replica == true)
                        {
                            WorkerService = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), existingW.address);
                            try
                            {
                                WorkerService.removeWorkerService(existingW.id);
                            }
                            catch (Exception e2) { }
                        }
                    }

                    //Calcula tempo medio por task de todo o SubJob
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
                        if ((2 * average_time_to_complete_task) < w.timePerTask)
                        {

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
                }
                else
                {
                    Console.WriteLine("Empty Thread running");
                }
                //if end
                System.Threading.Thread.Sleep(5000);//espera 5seg
            }
        }


        //SubmitJob Threads
        public void submitJobThread(Object arg)
        {
            JobArguments job = (JobArguments)arg;
            WorkerInterfaceRef service = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), job.address);
            service.submitSubJobService(job.nbr_splits, job.clientAddress, job.text_file, job.startingSplit_nbr);

        }

        //Executa um conunto de tasks
        public void executeSubJobThread(object arg)
        {
            this.ready = false;
            ClientInterface service = (ClientInterface)Activator.GetObject(typeof(ClientInterface), subJobW.clientAddress);

            foreach (int taskId in this.subJobW.taskList)
            {
                execute_task(taskId, subJobW.text_file, service);
            }
            this.ready = true;
        }

        /**************************
         * MAIN
        ***************************/
        static void Main(string[] args)
        {
            System.Console.WriteLine(args[0]);
            System.Console.WriteLine(args[1]);
            System.Console.WriteLine(args[2]);
            if (args.Length > 3)
            {
                System.Console.WriteLine(args[3]);
            }

            int id = Int32.Parse(args[0]);
            String pupperMasterURL = args[1];
            String serviceURL = args[2];
            String entryURL = "";
            if (args.Length > 3)
            {
                entryURL = args[3];
            }

          //Program JT1 = new Program(id, pupperMasterURL, serviceURL, entryURL);
          //Program JT1 = new Program(3, "tcp://localhost:10001/W", "tcp://localhost:30003/W", "tcp://localhost:30001/W");

            System.Console.ReadLine();

        }
    }




    /********************************
        * Services
    *********************************/
    //Serviços disponibilizadios Pelo no
    internal class WorkerServicesRef : MarshalByRefObject, WorkerInterfaceRef
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
                int subJobCounter = 0;
                foreach (int subjobSize in subJobSizeList)//enviar cada subJob a cada um dos JobTrackers
                {
                    if (p.jobtracker_list[i].id == p.id)//Se é o proprio
                    {
                        p.submitSubJob(subjobSize, client_address, text_file, subJobCounter);
                        subJobCounter += subjobSize;
                    }
                    else
                    { //Se é outro
                        //SERVICES
                        //lança TRHEAD para enviar o Job aos JT
                        ThreadPool.QueueUserWorkItem(new WaitCallback(p.submitJobThread), new JobArguments(subjobSize, p.jobtracker_list[i].address, client_address, text_file, subJobCounter));
                        subJobCounter += subjobSize;
                    }
                    i++;
                }
                response = "ok";
            }


            return response;//string com o address do JT o com "ok" para o cliente perceber que fo pedido esta a ser executado
        }

        //Permite que o Job seja dividido por vários JobTrackers
        public void submitSubJobService(int split_number, String client_address, string text_file, int startingSplit_nbr)
        {
            p.submitSubJob(split_number, client_address, text_file, startingSplit_nbr);
        }




        public int getId()
        {
            return p.id;
        }
        /***************************
         * WORKER SERVICES
         * ************************/
        //permite atribuir uma task ao Worker
        public void attributeTaskService(SubJobW subjobw)
        {
            p.subJobW = subjobw;
            ThreadPool.QueueUserWorkItem(new WaitCallback(p.executeSubJobThread), new object());
        }

        //permite conhecer o informação sobre o Worker e permite saber se este está vivo
        public WorkerState askNodeInfoService()
        {
            WorkerState ws;
            int taskNumber = 0;
            String assignedJTaddress = "none";
            String assignedReplicaAddress = "none";
            String nodeType = "";
            int workersNbr = 0;
            int jobtrackerNbr = 0;
            if (p.JT == true && p.W == false)//JobTracker
            {
                nodeType = "JT";
                if (p.replica != null)
                {
                    assignedReplicaAddress = p.replica.address;
                }
                jobtrackerNbr = p.jobtracker_list.Count;
                workersNbr = p.workers_list.Count;
            }
            else if (p.JT == true && p.W == true)//Worker Replica
            {
                if (p.subJobW != null)
                {
                    taskNumber = p.subJobW.taskList.Count;
                }
                assignedJTaddress = p.assignedJobTracker.address;
                nodeType = "WR";
                jobtrackerNbr = p.jobtracker_list.Count;
                workersNbr = p.workers_list.Count;
            }
            else if (p.JT == false && p.W == true)//Worker
            {
                if (p.subJobW != null)
                {
                    taskNumber = p.subJobW.taskList.Count;
                }
                assignedJTaddress = p.assignedJobTracker.address;
                nodeType = "W";
            }

            ws = new WorkerState(p.ready, p.freeze, taskNumber, assignedJTaddress, assignedReplicaAddress, nodeType, workersNbr, jobtrackerNbr);
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
        public String registerWorkerService(int id, String workerURL)//tem que responder ao Worker qual o id/ip do Jobtracker que este se deve associar
        {
            String workerFunction;

            System.Console.WriteLine(p.id);
            System.Console.WriteLine(p.address);
            WorkerInterfaceRef workerReplicaService;
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
                    p.workers_list.Add(new Worker(workerURL, id, true));

                }
                else
                {
                    workerFunction = "W";
                    Worker w = new Worker(workerURL, id, false);
                    p.workers_list.Add(w);//adiciona worker à sua lista de workers

                    //Procura na sua lista de Workers qual a replica e envia a informação do novo worker
                    foreach (Worker workerInList in p.workers_list)
                    {

                        Console.WriteLine("Worker id and address", workerInList.id, workerInList.address);
                        if (workerInList.replica == true)
                        {
                            workerReplicaService = (WorkerInterfaceRef)Activator.GetObject(typeof(WorkerInterfaceRef), workerInList.address);
                            workerReplicaService.addWorkerService(w);
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
        public List<JobTracker> getJTlistService()
        {
            return p.jobtracker_list;
        }

        //permite obter a lista de W
        public List<Worker> getWlistService()
        {
            System.Console.WriteLine(String.Concat("my id: " + p.id));
            System.Console.WriteLine(String.Concat("my address: " + p.address));
            return p.workers_list;
        }

        //permite que seja adicionao um novo JT á lista
        public void addJTService(JobTracker jt)
        {
            p.jobtracker_list.Add(jt);
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
        public void addWorkerService(Worker w)
        {
            p.workers_list.Add(w);
        }

        //permite conhecer o informação sobre o Worker e permite saber se este está vivo
        public void addSubJobList(List<SubJobW> subjobList)
        {
            p.subJobW_list = subjobList;
        }

        //actualiza o jobTracker
        public void updateJobTracker(JobTracker jt)
        {
            p.assignedJobTracker = jt;
        }
    }
}
