using System;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace DrawingSpace
{
    public class Samples
    {
        [CommandMethod("DSADDENTITY")]
        public static void AddEntity()
        {
            Line line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            DrawingSpace.AddEntity(line);
        }

        [CommandMethod("DSADDENTITYTRANS")]
        public static void AddEntityWithTransaction()
        {
            Transaction transaction = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction();
            Line line = new Line(new Point3d(0, 0, 0), new Point3d(1, 1, 0));
            DrawingSpace.AddEntity(line, transaction);

            // Since the transaction is still open we can continue to manipulate the object
            // and get access to properties that are set only after adding it to the database.
            line.Color = Color.FromColorIndex(ColorMethod.ByAci, 1);
            transaction.Commit();
        }

        [CommandMethod("DSGETENTITY")]
        public static void GetEntity()
        {
            Transaction transaction = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction();
            PromptStatus status = new PromptStatus();
            Entity entity = DrawingSpace.GetEntity("Select object:", transaction, ref status);

            // The status lets us know if the user actually selected something or cancelled.
            if (status == PromptStatus.OK)
            {
                entity.Color = Color.FromColorIndex(ColorMethod.ByAci, 2);
                transaction.Commit();
            }
            else
            {
                transaction.Dispose();
            }
        }

        [CommandMethod("DSGETSELECTION")]
        public static void GetSelection()
        {
            Transaction transaction = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction();
            PromptStatus status = new PromptStatus();
            DBObjectCollection selection = DrawingSpace.GetSelection("Select objects:", transaction, ref status);

            // Another way of dealing with the canceling of the operation or empty selection.
            if (status != PromptStatus.OK)
            {
                transaction.Dispose();
                return;
            }

            foreach (Entity entity in selection)
            {
                entity.Color = Color.FromColorIndex(ColorMethod.ByAci, 3);
            }

            transaction.Commit();
        }
    }
}
