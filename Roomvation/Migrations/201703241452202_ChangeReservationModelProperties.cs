namespace Roomvation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeReservationModelProperties : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Reservations", "Date", c => c.DateTime(nullable: false));
            AddColumn("dbo.Reservations", "StartTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.Reservations", "EndTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.Reservations", "StartDateTime");
            DropColumn("dbo.Reservations", "EndDateTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Reservations", "EndDateTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.Reservations", "StartDateTime", c => c.DateTime(nullable: false));
            DropColumn("dbo.Reservations", "EndTime");
            DropColumn("dbo.Reservations", "StartTime");
            DropColumn("dbo.Reservations", "Date");
        }
    }
}
