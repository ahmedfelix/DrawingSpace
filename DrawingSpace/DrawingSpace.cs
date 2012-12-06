using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;

namespace DrawingSpace
{
    static class DrawingSpace
    {
        /// <summary>
        /// Adds an entity to the Model space or current layout.
        /// </summary>
        public static void AddEntity(Entity entity)
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Transaction transaction = database.TransactionManager.StartTransaction();

            try
            {
                AddEntity(entity, transaction);
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
        public static void AddEntity(Entity entity, Transaction transaction)
        {
            Database database = HostApplicationServices.WorkingDatabase;
            BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId,
                OpenMode.ForRead);
            BlockTableRecord record = (BlockTableRecord)transaction.GetObject(database.CurrentSpaceId,
                OpenMode.ForWrite);

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
        /// Adds a new layer to the drawing.
        /// </summary>
        /// <param name="name">Name for the new layer.</param>
        /// <remarks>The layer only gets added if it doesn't exist, there's no exception 
        /// for trying to add an existing layer. The layer color will be white, 
        /// every other property is set to its default value.</remarks>
        public static void AddLayer(String name)
        {
            // 7 is the index for white, which is the default color.
            AddLayer(name, Color.FromColorIndex(ColorMethod.ByAci,7));
        }


        /// <summary>
        /// Adds a new layer to the drawing.
        /// </summary>
        /// <param name="name">Name for the new layer.</param>
        /// <remarks>The layer only gets added if it doesn't exist, there's no exception 
        /// for trying to add an existing layer.</remarks>
        public static void AddLayer(String name, Color color)
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Transaction transaction = database.TransactionManager.StartTransaction();

            LayerTable layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForWrite);
            LayerTableRecord layerRecord = new LayerTableRecord();

            layerRecord.Name = name;
            layerRecord.Color = color;

            try
            {
                if (layerTable.Has(name) == false)
                {
                    layerTable.Add(layerRecord);
                    transaction.AddNewlyCreatedDBObject(layerRecord, true);
                    transaction.Commit();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                layerRecord.Dispose();
                transaction.Dispose();
            }
        }

        /// <summary>
        /// Prompts the user to select a single entity from the drawing.
        /// </summary>
        /// <param name="prompt">Message to display in the command line.</param>
        public static Entity GetEntity(String prompt, Transaction transaction, ref PromptStatus status,
            OpenMode mode)
        {
            // The defaults for opening erased or locked objects are both false.
            return GetEntity(prompt, transaction, ref status, mode, false, false);
        }


        /// <summary>
        /// Prompts the user to select a single entity from the drawing.
        /// </summary>
        /// <param name="prompt">Message to display in the command line.</param>
        public static Entity GetEntity(String prompt, Transaction transaction, ref PromptStatus status,
            OpenMode mode, bool openErased)
        {
            // The default for opening objects in a locked layer is false.
            return GetEntity(prompt, transaction, ref status, mode, openErased, false);
        }


        /// <summary>
        /// Prompts the user to select a single entity from the drawing.
        /// </summary>
        /// <param name="prompt">Message to display in the command line.</param>
        public static Entity GetEntity(String prompt, Transaction transaction, ref PromptStatus status,
            OpenMode mode, bool openErased, bool forceOpenOnLockedLayer)
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
                entity = (Entity)transaction.GetObject(result.ObjectId, mode, openErased,
                    forceOpenOnLockedLayer);
            }

            return entity;
        }


        /// <summary>
        /// Gets an entity from a handle value.
        /// </summary>
        /// <returns>The entity that is identified in the drawing by the specified handle.</returns>
        public static Entity GetEntityByHandle(String handle, Transaction transaction, OpenMode mode)
        {
            // The defaults for opening erased or locked objects are both false.
            return GetEntityByHandle(handle, transaction, mode, false, false);
        }


        /// <summary>
        /// Gets an entity from a handle value.
        /// </summary>
        /// <returns>The entity that is identified in the drawing by the specified handle.</returns>
        public static Entity GetEntityByHandle(String handle, Transaction transaction,
            OpenMode mode, bool openErased)
        {
            // The default for opening objects in a locked layer is false.
            return GetEntityByHandle(handle, transaction, mode, openErased, false);
        }


        /// <summary>
        /// Gets an entity from a handle value.
        /// </summary>
        /// <returns>The entity that is identified in the drawing by the specified handle.</returns>
        public static Entity GetEntityByHandle(String handle, Transaction transaction,
            OpenMode mode, bool openErased, bool forceOpenOnLockedLayer)
        {
            Database database = HostApplicationServices.WorkingDatabase;
            ObjectId id = database.GetObjectId(false, new Handle(Convert.ToInt64(handle, 16)), 0);
            Entity entity;

            try
            {
                entity = (Entity)transaction.GetObject(id, mode, openErased, forceOpenOnLockedLayer);
            }
            catch (Exception)
            {
                throw;
            }

            return entity;
        }


        /// <summary>
        /// Prompts the user to select a point in the drawing.
        /// </summary>
        /// <param name="prompt">Message to display in the command line.</param>
        public static Point3d GetPoint3d(String prompt, ref PromptStatus status)
        {
            PromptPointOptions options = new PromptPointOptions(prompt);
            PromptPointResult result;
            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;

            options.AllowNone = true;
            result = command.GetPoint(options);
            status = result.Status;

            return result.Value;
        }


        /// <summary>
        /// Prompts the user to do a selection of one or more objects.
        /// </summary>
        /// <param name="prompt">Message to display in the command line.</param>
        public static DBObjectCollection GetSelection(String prompt, Transaction transaction,
            ref PromptStatus status, OpenMode mode)
        {
            // The defaults for opening erased or locked objects are both false.
            return GetSelection(prompt, transaction, ref status, mode, false, false);
        }


        /// <summary>
        /// Prompts the user to do a selection of one or more objects.
        /// </summary>
        /// <param name="prompt">Message to display in the command line.</param>
        public static DBObjectCollection GetSelection(String prompt, Transaction transaction,
            ref PromptStatus status, OpenMode mode, bool openErased)
        {
            // The default for opening objects in a locked layer is false.
            return GetSelection(prompt, transaction, ref status, mode, openErased, false);
        }


        /// <summary>
        /// Prompts the user to do a selection of one or more objects.
        /// </summary>
        /// <param name="prompt">Message to display in the command line.</param>
        public static DBObjectCollection GetSelection(String prompt, Transaction transaction,
            ref PromptStatus status, OpenMode mode, bool openErased, bool forceOpenOnLockedLayer)
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
                    dbObject = transaction.GetObject(id, mode, openErased, forceOpenOnLockedLayer);
                    objectsSelected.Add(dbObject);
                }
            }

            return objectsSelected;
        }
    }
}