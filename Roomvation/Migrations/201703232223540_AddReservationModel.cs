namespace Roomvation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReservationModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Participations",
                c => new
                    {
                        ReservationId = c.Int(nullable: false),
                        ParticipantId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ReservationId, t.ParticipantId })
                .ForeignKey("dbo.AspNetUsers", t => t.ParticipantId)
                .ForeignKey("dbo.Reservations", t => t.ReservationId, cascadeDelete: true)
                .Index(t => t.ReservationId)
                .Index(t => t.ParticipantId);
            
            CreateTable(
                "dbo.Reservations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(nullable: false),
                        MeetingDescription = c.String(nullable: false, maxLength: 250),
                        CreationDate = c.DateTime(nullable: false),
                        CreatorId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatorId)
                .Index(t => t.CreatorId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Participations", "ReservationId", "dbo.Reservations");
            DropForeignKey("dbo.Reservations", "CreatorId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Participations", "ParticipantId", "dbo.AspNetUsers");
            DropIndex("dbo.Reservations", new[] { "CreatorId" });
            DropIndex("dbo.Participations", new[] { "ParticipantId" });
            DropIndex("dbo.Participations", new[] { "ReservationId" });
            DropTable("dbo.Reservations");
            DropTable("dbo.Participations");
        }
    }
}
