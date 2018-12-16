﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MY_BE;
using MY_DAL;
namespace MY_BL
{
    public class Bl_imp:IBL
    {
        Idal d = new Dal_imp();
        public void addTester(MY_BE.Tester tester)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - tester.BirthDate.Year;
            if (tester.BirthDate > today.AddYears(-1*age))
            {
                age--;
            }
            if (age>=40)
            {
                d.addTester(tester);
            }
            else throw new Exception("tried to insert tester younger than 40");
        }
        public void removeTester(MY_BE.Tester tester)
        {
            bool tester_exists = false;
            foreach(Tester item in getAllTesters())
            {
                if (item.id == tester.id)
                {
                    tester_exists = true;
                    d.removeTester(tester);
                }
            }
        }
        public void updateTester(MY_BE.Tester tester)
        {

        }
        public void addTrainee(MY_BE.Trainee trainee)
        {
            int traineeBirthYear = int.Parse(trainee.BirthDate.Year.ToString());
            int currentYear = int.Parse(DateTime.Now.Year.ToString());
            if (currentYear - traineeBirthYear > 18)
            {
                d.addTrainee(trainee);
            }
            else throw new Exception("tried to insert trainee younger than 18");
        }
        public void removeTrainee(MY_BE.Trainee trainee)
        {

        }
        public void updateTrainee(MY_BE.Trainee trainee)
        {

        }
        public void addTest(MY_BE.Test test)
        {

            bool canDoTest = true;
            Trainee newTrainee = new Trainee();
            foreach (var item in getAllTrainees())//check if trainee is valid for test
            {
                if (item.id == test.TraineeId)
                {
                    newTrainee = item;// בשביל הבדיקה הראשונה שהייתי צריך לעשות
                    if (item.TraineeVehicle!=test.TraineeVehicle)
                    {
                        canDoTest = false;
                        throw new Exception("The Trainee is tested on other vehicles");
                    }//trainee vehicle is the same as test vehicle
                    else if (item.TraineeVehicle == test.TraineeVehicle && item.passedTheTest==true)
                    {
                        canDoTest = false;
                        throw new Exception("The Trainee has already passed the test");
                    }//if the trainee already passed the test
                    if ((test.TestDateTime - item.TestDay).TotalDays <= 7)
                    {
                        canDoTest = false;
                        throw new Exception("you need to wait 7 days between tests");
                    }//a week between the last test and current test
                    if (item.DrivingLessonsNumber < 20)
                    {
                        canDoTest = false;
                        throw new Exception("atleast 20 lessons required to make a test");
                    }//check if the trainee did less than 20 lessons
                    break;//only one student with matching id in the list
                }
            }
            foreach (var item in getAllTesters())//look for available tester
            {
                if (!item.weekdays[test.TestDateTime.DayOfWeek, test.TestDateTime.Hour])
                {
                    canDoTest = false;
                    try
                    {
                        item.nextAvailableTest();
                    }
                    catch(Exception e)
                    {
                        throw new Exception("the selected hour is already taken.\n" + e.Message);
                    }
                    break;
                }//check if the test hour is not taken
                if (item.MaxWeeklyTests <= item.weekdays.currentWeeklyTests)
                {
                    canDoTest = false;
                    break;
                }//check if the tester passed the max weekly tests 
                if (newTrainee.TraineeVehicle!=item.TesterVehicle)
                {
                    canDoTest = false;
                    throw new Exception("The tester can test on another type of vehicle");
                    break;
                }// בשביל הבדיקה השנייה שהייתי צריך לעשות
                if (canDoTest)
                {
                    test.TesterId = item.id;
                    break;
                }//if the tester is available for testing assign the tester for the test
            }

            if (canDoTest)
            {
                d.addTest(test);
            }
        }
        public void updateTestOnFinish(MY_BE.Test test)
        {
            bool canupdatetest = true;
            foreach(var item in test.TestParams)//check if all testparams was entered false\true
            {
                if (item.Value == null)
                {
                    canupdatetest = false;
                }
            }
            if (canupdatetest)
            {
                d.updateTestOnFinish(test);
            }
        }
        public List<MY_BE.Tester> getAllTesters()
        {
            return d.getAllTesters();
        }
        public List<MY_BE.Trainee> getAllTrainees()
        {
            return d.getAllTrainees();
        }
        public List<MY_BE.Test> getAllTests()
        {
            return d.getAllTests();
        }
        public List<Tester> getNearbyTesters(Adress ad,double x)
        {
            List<Tester> tlist=new List<Tester>();
            double distance = 4;
            foreach(Tester t in getAllTesters())
            {
                //enter gmaps distance to distance
                if (distance <= x)
                {
                    tlist.Add(t);
                }
            }
            return tlist;
        }
        public List<Tester> getTestersAtHour(DateTime dt)
        {
            List<Tester> tlist = new List<Tester>();
            foreach (var item in getAllTesters())
            {
                if (item.weekdays[dt.DayOfWeek, dt.Hour])
                {
                    tlist.Add(item);
                }
            }
            return tlist;
        }
        //**public delegate bool testcheckif(Test t);
        /*public List<Test> testsCheck(testcheckif d)
        {
            List<Test> tlist = new List<Test>();
            foreach (Test t in getAllTests())
            {
                if (d(t))
                {

                }
            }
            return tlist;
        }//return all the tests that return true for the delegates checks*/
        public int numOfTests(Trainee trainee)
        {
            int sum = 0;
            foreach(Test item in getAllTests())
            {
                if (item.TraineeId == trainee.id)
                {
                    sum++;
                }
            }
            return sum;
        }
        public bool passedTheTest(Trainee trainee)
        {
            return trainee.passedTheTest;
        }
        public List<Test> testsOnDate(DateTime dt)
        {
            List < Test > tlist= new List<Test>();

            var results = from item in getAllTests()
                          where (dt.Month == item.TestDate.Month && dt.Day == item.TestDate.Day)
                          select item;
            foreach (Test item in results)
            {
                tlist.Add(item);
            }
            return tlist;
        }
        public List<Tester> groupByVehicleType()
        {
            List<Tester> tlist = new List<Tester>();
            var results = getAllTesters().GroupBy(item => item.TesterVehicle);
            /*var results2 = from item in getAllTests()
                          group item by item.TraineeVehicle;
            */
            foreach (IGrouping<VehicleType,Tester> group in results)
            {
                foreach(Tester item in group)
                {
                    tlist.Add(item);
                }
            }
            return tlist;
        }
        public List<Trainee> groupByDrivingSchool()
        {
            List<Trainee> tlist = new List<Trainee>();
            var results = getAllTrainees().GroupBy(item => item.DrivingSchool);
            foreach(IGrouping<string,Trainee> group in results)
            {
                foreach(Trainee item in group)
                {
                    tlist.Add(item);
                }
            }
            return tlist;
        }
        public List<Trainee> groupByTester()
        {
            List<Trainee> tlist = new List<Trainee>();
            var results = getAllTrainees().GroupBy(item => item.TeacherName);
            foreach (IGrouping<string, Trainee> group in results)
            {
                foreach (Trainee item in group)
                {
                    tlist.Add(item);
                }
            }
            return tlist;
        }
        public List<Trainee> groupByTestsNumber()
        {
            List<Trainee> tlist = new List<Trainee>();
            var results = getAllTrainees().GroupBy(item => item);//**need to make tests number field in trainee
            foreach (IGrouping<string, Trainee> group in results)
            {
                foreach (Trainee item in group)
                {
                    tlist.Add(item);
                }
            }
            return tlist;
        }
    }

}
