using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ParkEasyV1.Models
{
    /// <summary>
    /// Class used to initialize the database and seed data using the DropCreateDatabaseAlways method for testing
    /// Updated: Using DropCreatDatabaseIfModelChanges for live version
    /// </summary>
    public class DatabaseInitialiser : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        /// <summary>
        /// Override Seed method to seed the database with values
        /// </summary>
        /// <param name="context"></param>
        protected override void Seed(ApplicationDbContext context)
        {
            //call to base with ApplicationDbContext
            base.Seed(context);

            //save changes
            context.SaveChanges();

            //Call to method to create the 150 initial parking slots
            CreateParkingSlots(context);
        }//end method

        /// <summary>
        /// Method to create initial 150 parking slots in the system
        /// </summary>
        /// <param name="context">ApplicationDbContext</param>
        private void CreateParkingSlots(ApplicationDbContext context)
        {
            var parkingSlotId = 1;
            for (int floorNumber = 1; floorNumber <= 3 ; floorNumber++)
            {
                switch (floorNumber)
                {
                    case 1:
                        for (int parkingSlotNumber = 1; parkingSlotNumber <= 155; parkingSlotNumber++)
                        {
                            //create new available parking slot
                            context.ParkingSlots.Add(new ParkingSlot()
                            {
                                ID = parkingSlotId,
                                FloorNu = floorNumber,
                                Status = Status.Available,
                                ParkingSlotNumber = parkingSlotNumber
                            });
                            parkingSlotId++;
                        }
                        break;
                    case 2:
                        for (int parkingSlotNumber = 1; parkingSlotNumber <= 160; parkingSlotNumber++)
                        {
                            //create new available parking slot
                            context.ParkingSlots.Add(new ParkingSlot()
                            {
                                ID = parkingSlotId,
                                FloorNu = floorNumber,
                                Status = Status.Available,
                                ParkingSlotNumber = parkingSlotNumber
                            });
                            parkingSlotId++;
                        }
                        break;
                    case 3:
                        for (int parkingSlotNumber = 1; parkingSlotNumber <= 178; parkingSlotNumber++)
                        {
                            //create new available parking slot
                            context.ParkingSlots.Add(new ParkingSlot()
                            {
                                ID = parkingSlotId,
                                FloorNu = floorNumber,
                                Status = Status.Available,
                                ParkingSlotNumber = parkingSlotNumber
                            });
                            parkingSlotId++;
                        }
                        break;
                    default:
                        break;
                }
            }           

            //save changes
            context.SaveChanges();
        }
    }
}