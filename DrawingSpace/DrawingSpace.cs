using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace DrawingSpace
{
    static class DrawingSpace
    {

        /// <summary>
        /// Adds an entity to the Model space or current layout.
        /// </summary>
        /// <param name="entity"></param>
        public static void AddEntity(Entity entity)
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Transaction transaction = database.TransactionManager.StartTransaction();
            BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord record = (BlockTableRecord)transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite);

            try
            {
                record.AppendEntity(entity);
                transaction.AddNewlyCreatedDBObject(entity, true);
                transaction.Commit();
            }

            catch (Exception)
            {
                throw;
            }

            finally
            {
                transaction.Dispose();
                entity.Dispose();
            }
        }

        /// <summary>
        /// Adds an entity to the Model space or current layout, keeping the transaction open
        /// so the entity can continue being used.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="transaction"></param>
        public static void AddEntity(Entity entity, Transaction transaction)
        {
            Database database = HostApplicationServices.WorkingDatabase;
            BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord record = (BlockTableRecord)transaction.GetObject(database.CurrentSpaceId, OpenMode.ForWrite);

            try
            {
                record.AppendEntity(entity);
                transaction.AddNewlyCreatedDBObject(entity, true);
            }

            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// Prompts the user to select a single entity from the drawing.
        /// </summary>
        /// <param name="prompt">Message to display in the command line.</param>
        /// <param name="transaction"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static Entity GetEntity(String prompt, Transaction transaction, ref PromptStatus status)
        {
            PromptEntityOptions options = new PromptEntityOptions(System.Environment.NewLine + prompt);
            PromptEntityResult result;
            Entity entity = null;
            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;

            options.AllowNone = true;
            result = command.GetEntity(options);
            status = result.Status;

            if (status == PromptStatus.OK)
            {
                entity = (Entity)transaction.GetObject(result.ObjectId, OpenMode.ForWrite);
            }

            return entity;
        }

        /// <summary>
        /// Prompts the user to do a selection of one or more objects.
        /// </summary>
        /// <param name="prompt">Message to display in the command line.</param>
        /// <param name="transaction"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static DBObjectCollection GetSelection(String prompt, Transaction transaction, ref PromptStatus status)
        {
            PromptSelectionOptions options = new PromptSelectionOptions();
            PromptSelectionResult result;
            SelectionSet selection;
            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;
            DBObjectCollection objectsSelected = new DBObjectCollection();

            options.MessageForAdding = System.Environment.NewLine + prompt;
            result = command.GetSelection(options);
            status = result.Status;

            ObjectId[] idArray;
            DBObject dbObject;

            selection = result.Value;

            if (selection != null)
            {
                idArray = selection.GetObjectIds();

                foreach (ObjectId id in idArray)
                {
                    dbObject = transaction.GetObject(id, OpenMode.ForWrite, false, true);
                    objectsSelected.Add(dbObject);
                }
            }

            return objectsSelected;
        }
    }
}