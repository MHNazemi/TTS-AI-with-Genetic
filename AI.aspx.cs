using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Modiran_21_Management_System.Bisunes_Layer;
using System.Data;

namespace Modiran_21_Management_System
{
    public partial class AI : System.Web.UI.Page
    {
        DateTime dateCurrentWeekStart;
        DateTime dateCurrentWeekEnd;
        protected void Page_Load(object sender, EventArgs e)
        {
            ClsPublic objPublic = new ClsPublic();
            //dateCurrentWeekStart = objPublic.DateTimeOfStartOfWeek();
            //dateCurrentWeekEnd = objPublic.DateTimeOfEndOfWeek();
            dateCurrentWeekStart = objPublic.DateTimeOfStartOfWeek(DateTime.Parse("2016-08-10"));
            dateCurrentWeekEnd = objPublic.DateTimeOfEndOfWeek(DateTime.Parse("2016-08-10"));
        }






        Random mainRandom = new Random();
        Dictionary<int, Meeting> meetings;
        List<int> meetingsIDs;
        Chromosome[] chromosomes;
        List<Chromosome> constantChromosomes = new List<Chromosome>();

        const int populationSize = 4000;
        const int generation = 20;
        const int idPerson = 59;
        const int samePlacePoint = 10;
        const int constraintMeetingsFitness = -3;



        protected void btnGeneticCore1_Click(object sender, EventArgs e)
        {

            TimeTable.Clear();
            ClsPreMeetingTable objPreMeeting = new ClsPreMeetingTable();

            DataTable dat = objPreMeeting.GetDataTableSelectedMeetingDetailsOfThisWeek(idPerson, dateCurrentWeekStart, dateCurrentWeekEnd);

            //we will use this dictinary as a abstract data of meetings
            meetings = InitilizeData(dat);

            //making a list of avalilabe IDMeetings
            meetingsIDs = new List<int>();
            foreach (int key in meetings.Keys)
            {
                meetingsIDs.Add(key);
            }

            InitilizePopulation(populationSize);
            FetchConstantChromosomes(idPerson);

            StartGeneration(generation);


        }

        public Dictionary<int, Meeting> InitilizeData(DataTable dat)
        {
            Dictionary<int, Meeting> meetings = new Dictionary<int, Meeting>();
            Meeting meeting;
            foreach (DataRow myRow in dat.Rows)
            {
                meeting = new Meeting { IDMeeting = myRow["IDEntity"].ToString().ToInt(), Duraion = myRow["Duration"].ToString().ToInt() / 15, Event = calculateLimitation(myRow["IDEntity"].ToString().ToInt()), IDPlace = myRow["PlaceCompanyID"].ToString().ToInt() };
                meetings.Add(myRow["IDEntity"].ToString().ToInt(), meeting);
            }

            return meetings;
        }

        public bool[] calculateLimitation(int idMeeting)
        {
            ClsMeetingRequest objMeetingRequest = new ClsMeetingRequest();
            ClsMeetingRequestPerson objMeetingRequestPerson = new ClsMeetingRequestPerson();
            ClsLimitation objLimitation = new ClsLimitation();
            ClsMeetingTable objMeetingTable = new ClsMeetingTable();



            int idPlace = objMeetingRequest.SelectByIDMeeting(idMeeting).Rows[0]["PlaceCompanyID"].ToString().ToInt();


            bool[] dropZoneMap = Enumerable.Repeat<bool>(true, 288).ToArray();
            DataTable dTableMeetinngPerson = objMeetingRequestPerson.GetDataTableAllPersonByIDMeeetingRequest(idMeeting);
            DataTable dTableLimitation;
            DataTable dTableMeetingTable;
            int daysOffset = 0;
            int hourOffsetStart = 0;
            int minOffsetStart = 0;
            int hourOffsetEnd = 0;
            int minOffsetEnd = 0;
            foreach (DataRow dRow in dTableMeetinngPerson.Rows)
            {
                dTableLimitation = objLimitation.GetDataTableLimitationByIDPersonInCurrentWeek(dateCurrentWeekStart, dateCurrentWeekEnd, int.Parse(dRow["NO"].ToString()), idPlace);
                foreach (DataRow dRowInner in dTableLimitation.Rows)
                {
                    daysOffset = int.Parse(dRowInner["Day"].ToString());
                    daysOffset = (daysOffset * 48);
                    hourOffsetStart = int.Parse(dRowInner["TimeFrom"].ToString().Split(':')[0]);
                    hourOffsetStart = hourOffsetStart - 8;
                    hourOffsetStart = hourOffsetStart * 4;
                    minOffsetStart = int.Parse(dRowInner["TimeFrom"].ToString().Split(':')[1]);
                    switch (minOffsetStart)
                    {
                        case 0:
                            minOffsetStart = 0;
                            break;
                        case 15:
                            minOffsetStart = 1;
                            break;
                        case 30:
                            minOffsetStart = 2;
                            break;
                        case 45:
                            minOffsetStart = 3;
                            break;
                        default:
                            minOffsetStart = 0;
                            break;
                    }


                    hourOffsetEnd = int.Parse(dRowInner["TimeTo"].ToString().Split(':')[0]);
                    hourOffsetEnd = hourOffsetEnd - 8;
                    hourOffsetEnd = hourOffsetEnd * 4;
                    minOffsetEnd = int.Parse(dRowInner["TimeTo"].ToString().Split(':')[1]) / 15;

                    for (int j = daysOffset + hourOffsetStart + minOffsetStart; j < daysOffset + hourOffsetEnd + minOffsetEnd; j++)
                    {
                        dropZoneMap[j] = false;
                    }

                }


                dTableMeetingTable = objMeetingTable.GetDataTableAcctepedMeetingOfThisWeek(dateCurrentWeekStart, dateCurrentWeekEnd, dRow["NO"].ToString().ToInt(), idMeeting);
                foreach (DataRow dRowInner in dTableMeetingTable.Rows)
                {
                    daysOffset = int.Parse(dRowInner["Day"].ToString());
                    daysOffset = (daysOffset * 48);
                    hourOffsetStart = int.Parse(dRowInner["TimeFrom"].ToString().Split(':')[0]);
                    hourOffsetStart = hourOffsetStart - 8;
                    hourOffsetStart = hourOffsetStart * 4;
                    minOffsetStart = int.Parse(dRowInner["TimeFrom"].ToString().Split(':')[1]);
                    switch (minOffsetStart)
                    {
                        case 0:
                            minOffsetStart = 0;
                            break;
                        case 15:
                            minOffsetStart = 1;
                            break;
                        case 30:
                            minOffsetStart = 2;
                            break;
                        case 45:
                            minOffsetStart = 3;
                            break;
                        default:
                            minOffsetStart = 0;
                            break;
                    }


                    hourOffsetEnd = int.Parse(dRowInner["TimeTo"].ToString().Split(':')[0]);
                    hourOffsetEnd = hourOffsetEnd - 8;
                    hourOffsetEnd = hourOffsetEnd * 4;
                    minOffsetEnd = int.Parse(dRowInner["TimeTo"].ToString().Split(':')[1]) / 15;

                    for (int j = daysOffset + hourOffsetStart + minOffsetStart; j < daysOffset + hourOffsetEnd + minOffsetEnd; j++)
                    {
                        dropZoneMap[j] = false;
                    }
                }
            }
            return dropZoneMap;

        }

        public void InitilizePopulation(int count)
        {
            chromosomes = new Chromosome[count];
            int idRandomMeeting;
            TimeSlot tempSlot;
            for (int i = 0; i < count; i++)
            {
                idRandomMeeting = meetingsIDs[mainRandom.Next(0, meetingsIDs.Count)];
                tempSlot = CreateRandomSlotGene(idRandomMeeting);
                if (tempSlot == null)
                {
                    i--;
                    continue;
                }
                chromosomes[i] = new Chromosome() { IDMeeting = idRandomMeeting, Fitness = 1, Slot = tempSlot };
            }
        }

        public TimeSlot CreateRandomSlotGene(int idMeeting)
        {
            TimeSlot slot = new TimeSlot();
            Meeting myMeeting = meetings[idMeeting];


            //we use list over dictionary becasue we want to choose randomly between postions;
            List<Tuple<int, int>> availableBools = new List<Tuple<int, int>>();

            Tuple<int, int> tempTuple;

            bool tempKeepState = false;
            int tempStartIndex = 0;
            for (int i = 0; i < myMeeting.Event.Length; i++)
            {
                if (myMeeting.Event[i] && !tempKeepState)
                {
                    tempKeepState = true;
                    tempStartIndex = i;
                }

                if (!myMeeting.Event[i] && tempKeepState)
                {
                    tempKeepState = false;
                    tempTuple = new Tuple<int, int>(tempStartIndex, i - 1);
                    availableBools.Add(tempTuple);
                }

            }

            if (tempKeepState)
            {
                tempKeepState = false;
                tempTuple = new Tuple<int, int>(tempStartIndex, 287);
                availableBools.Add(tempTuple);
            }

            availableBools = availableBools.Where(x => (x.Item2 - x.Item1) + 1 >= myMeeting.Duraion).ToList<Tuple<int, int>>();

            if (availableBools.Count == 0)
                return null;

            tempTuple = availableBools[mainRandom.Next(0, availableBools.Count)];

            slot.Start = mainRandom.Next(tempTuple.Item1, tempTuple.Item2 - myMeeting.Duraion + 2);
            slot.End = slot.Start + myMeeting.Duraion - 1;

            return slot;
        }

        public void FetchConstantChromosomes(int IDPerson)
        {
            ClsMeetingTable objMeetingTable = new ClsMeetingTable();
            ClsMeetingRequestPerson objMeetingRequestPerson = new ClsMeetingRequestPerson();
            Meeting myMeeting;
            Chromosome myChromosome;

            foreach (DataRow dRow in objMeetingTable.GetDataTableAcceptedMeetingsOfWeekWithDetailsofCompanyAndCompanyPlaceAndMeetingRequestWithIDPerson(dateCurrentWeekStart, dateCurrentWeekEnd, IDPerson).Rows)
            {
                myMeeting = new Meeting();
                myChromosome = new Chromosome();
                myMeeting.IDMeeting = dRow["IDMeeting"].ToString().ToInt();
                myMeeting.IDPlace = dRow["PlaceCompanyID"].ToString().ToInt();
                myMeeting.Duraion = dRow["Duration"].ToString().ToInt();

                meetings.Add(myMeeting.IDMeeting, myMeeting);

                myChromosome.IDMeeting = myMeeting.IDMeeting;
                myChromosome.Fitness = constraintMeetingsFitness;
                myChromosome.Slot = new TimeSlot { Start = (dRow["Day"].ToString().ToInt() * 48) + ((dRow["TimeFrom"].ToString().Split(':')[0].ToInt() - 8) * 4) + (dRow["TimeFrom"].ToString().Split(':')[1].ToInt() / 15), End = (dRow["Day"].ToString().ToInt() * 48) - 1 + ((dRow["TimeTo"].ToString().Split(':')[0].ToInt() - 8) * 4) + (dRow["TimeTo"].ToString().Split(':')[1].ToInt() / 15) };

                constantChromosomes.Add(myChromosome);
            }
        }

        public void StartGeneration(int generationCount)
        {
            for (int i = 0; i < generationCount; i++)
            {
                CreateTable();

                InitilizePopulationBaseOnFitness(populationSize);
            }

            TimeTable t = TimeTable.GetBestTimeTable();
            ClsMeetingTable objMeetingTable = new ClsMeetingTable();
            ClsMeetingTable.strcMeetingTable myRowMeetingTable = objMeetingTable.GetNewRow();
            ClsPublic objPublic = new ClsPublic();
            foreach (Chromosome myChromosome in t.submittedChromosomes)
            {
                myRowMeetingTable.IDMeeting = myChromosome.IDMeeting;
                myRowMeetingTable.IDSubmitter = idPerson;
                myRowMeetingTable.Date = objPublic.DateTimeOfEndOfWeek(dateCurrentWeekEnd);
                myRowMeetingTable.Day = (short)(myChromosome.Slot.Start / 48);
                int temp = (myChromosome.Slot.Start % 48);
                temp *= 15;
                myRowMeetingTable.TimeFrom = TimeSpan.Parse(((temp / 60) + 8) + ":" + (temp % 60));

                temp = (myChromosome.Slot.End % 48);
                temp *= 15;
                myRowMeetingTable.TimeTo = TimeSpan.Parse(((temp / 60) + 8) + ":" + (temp % 60));

                objMeetingTable.Insert(myRowMeetingTable);

            }
        }

        public void AddConstantChromosomes(TimeTable timetable)
        {
            foreach (Chromosome myChromosome in constantChromosomes)
            {
                timetable.submittedChromosomes.Add(myChromosome);
            }
        }
        public void CreateTable()
        {
            //TimeTable.Clear();
            Chromosome[] tempChromsomes = chromosomes.Where(c => !c.Blocked && !c.Selected).ToArray<Chromosome>();
            Chromosome selectedChromosome;
            TimeTable timetable;


            while (tempChromsomes.Length != 0)
            {
                timetable = TimeTable.CreateTimeTable();
                while (tempChromsomes.Length != 0)
                {
                    selectedChromosome = tempChromsomes[mainRandom.Next(0, tempChromsomes.Length)];
                    chromosomes.Where(c => !c.Blocked && !c.Selected && (((c.Slot.Start <= selectedChromosome.Slot.Start && c.Slot.End >= selectedChromosome.Slot.Start) || (c.Slot.Start >= selectedChromosome.Slot.Start && c.Slot.End <= selectedChromosome.Slot.End) || (c.Slot.Start <= selectedChromosome.Slot.End && c.Slot.End >= selectedChromosome.Slot.End) || (c.Slot.Start <= selectedChromosome.Slot.Start && c.Slot.End >= selectedChromosome.Slot.End)) || c.IDMeeting == selectedChromosome.IDMeeting)).ToList<Chromosome>().ForEach(x => x.Blocked = true);
                    timetable.submittedChromosomes.Add(selectedChromosome);
                    selectedChromosome.Selected = true;
                    tempChromsomes = chromosomes.Where(c => !c.Blocked && !c.Selected).ToArray<Chromosome>();
                }
                FitnessFunction(timetable);

                chromosomes.Where(c => c.Blocked && !c.Selected).ToList<Chromosome>().ForEach(x => x.Blocked = false);
                tempChromsomes = chromosomes.Where(c => !c.Blocked && !c.Selected).ToArray<Chromosome>();

            }
        }

        /// <summary>
        /// Base on place of meetings
        /// </summary>
        public void FitnessFunction(TimeTable timeTable)
        {
            int distance = 0;
            List<Chromosome> evaluationChromosomes = timeTable.submittedChromosomes, tempChromosomesList;
            int tempStartSlot = 0, tempEndSlot;
            foreach (Chromosome selectedChromosome in evaluationChromosomes)
            {
                //-3 (constraintMeetingsFitness) means it's constant meetings
                if (selectedChromosome.Fitness != constraintMeetingsFitness)
                {
                    // Creating Boundries of a day
                    tempStartSlot = selectedChromosome.Slot.Start;
                    tempEndSlot = selectedChromosome.Slot.End;

                    tempStartSlot = (tempStartSlot / 48);
                    tempStartSlot = tempStartSlot * 48;
                    //tempStartSlot++;


                    tempEndSlot = tempEndSlot / 48;
                    tempEndSlot++;
                    tempEndSlot = tempEndSlot * 48;
                    tempEndSlot--;

                    tempChromosomesList = evaluationChromosomes.Where(x => x.Slot.Start < selectedChromosome.Slot.Start && x.Slot.Start >= tempStartSlot).OrderByDescending(c => c.Slot.Start).ToList<Chromosome>();

                    if (tempChromosomesList.Count > 0)
                    {

                        //In Destination Based Soft Constraints it should be check for destination
                        if (meetings[tempChromosomesList[0].IDMeeting].IDPlace == meetings[selectedChromosome.IDMeeting].IDPlace)
                        {
                            //restarting Fittness
                            if (selectedChromosome.Fitness == -1)
                                selectedChromosome.Fitness = 0;

                            distance = tempChromosomesList[0].Slot.End - selectedChromosome.Slot.Start;

                            if (distance < samePlacePoint)
                                selectedChromosome.Fitness += samePlacePoint - distance;

                            if (tempChromosomesList[0].Fitness != constraintMeetingsFitness)
                            {
                                if (distance < samePlacePoint)
                                    tempChromosomesList[0].Fitness += samePlacePoint - distance;
                            }

                        }


                    }


                }


            }
        }


        public void InitilizePopulationBaseOnFitness(int count)
        {
            //TimeTable.Clear();

            Chromosome[] lastGenerationChromosomes = chromosomes.Clone() as Chromosome[];
            int totalFitness = 0;
            lastGenerationChromosomes.ToList<Chromosome>().ForEach(c => { totalFitness += c.Fitness; c.Blocked = false; c.Selected = false; });
            lastGenerationChromosomes = lastGenerationChromosomes.OrderByDescending(c => c.Fitness).ToArray<Chromosome>();

            chromosomes = new Chromosome[count];

            int countOfGeneration = 0;
            int indexer = -1;
            for (int i = 0; i < lastGenerationChromosomes.Length; i++)
            {
                if (countOfGeneration == 0)
                {
                    indexer++;
                    countOfGeneration = (int)((((double)lastGenerationChromosomes[indexer].Fitness / (double)totalFitness)) * count);
                    if (countOfGeneration == 0)
                        countOfGeneration = 1;
                }
                chromosomes[i] = new Chromosome() { IDMeeting = lastGenerationChromosomes[indexer].IDMeeting, Fitness = 1 /*lastGenerationChromosomes[indexer].Fitness */, Slot = lastGenerationChromosomes[indexer].Slot };
                countOfGeneration--;
            }
        }



    }




}