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
        public static void AddLayer(string name)
        {
            // 7 is the index for white, which is the default color.
            AddLayer(name, Color.FromColorIndex(ColorMethod.ByAci, 7));
        }


        /// <summary>
        /// Adds a new layer to the drawing.
        /// </summary>
        /// <param name="name">Name for the new layer.</param>
        /// <remarks>The layer only gets added if it doesn't exist, there's no exception 
        /// for trying to add an existing layer.</remarks>
        public static void AddLayer(string name, Color color)
        {
            Database database = HostApplicationServices.WorkingDatabase;
            Transaction transaction = database.TransactionManager.StartTransaction();

            LayerTable layerTable = (LayerTable)transaction.GetObject(database.LayerTableId, OpenMode.ForWrite);
            LayerTableRecord layerRecord = new LayerTableRecord();

            layerRecord.Name = name;
            layerRecord.Color = color;

            try
            {
                if (!layerTable.Has(name))
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
        /// Prompts the user to input a double.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        /// <param name="allowNone">If false, the user is forced to enter a number or cancel.</param>
        /// <returns>The double the user wrote. Any other operation will return 0.</returns>
        public static double GetDouble(string message, ref PromptStatus status, bool allowNone)
        {
            // Default value is 0.
            return GetDouble(message, ref status, allowNone, 0.0);
        }


        /// <summary>
        /// Prompts the user to input a double.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        /// <param name="allowNone">If false, the user is forced to enter a number or cancel.</param>
        /// <returns>The double the user wrote. Any other operation will return the default value.</returns>
        public static double GetDouble(string message, ref PromptStatus status, bool allowNone,
            double defaultValue)
        {
            PromptDoubleOptions options = new PromptDoubleOptions(System.Environment.NewLine + message);
            options.AllowNone = allowNone;
            options.DefaultValue = defaultValue;
            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptDoubleResult result = command.GetDouble(options);
            status = result.Status;

            return result.Value;
        }


        /// <summary>
        /// Prompts the user to select a single entity from the drawing.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        public static Entity GetEntity(string message, Transaction transaction, ref PromptStatus status,
            OpenMode mode)
        {
            // The defaults for opening erased or locked objects are both false.
            return GetEntity(message, transaction, ref status, mode, false, false);
        }


        /// <summary>
        /// Prompts the user to select a single entity from the drawing.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        public static Entity GetEntity(string message, Transaction transaction, ref PromptStatus status,
            OpenMode mode, bool openErased)
        {
            // The default for opening objects in a locked layer is false.
            return GetEntity(message, transaction, ref status, mode, openErased, false);
        }


        /// <summary>
        /// Prompts the user to select a single entity from the drawing.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        public static Entity GetEntity(string message, Transaction transaction, ref PromptStatus status,
            OpenMode mode, bool openErased, bool forceOpenOnLockedLayer)
        {
            PromptEntityOptions options = new PromptEntityOptions(System.Environment.NewLine + message);
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
        public static Entity GetEntityByHandle(string handle, Transaction transaction, OpenMode mode)
        {
            // The defaults for opening erased or locked objects are both false.
            return GetEntityByHandle(handle, transaction, mode, false, false);
        }


        /// <summary>
        /// Gets an entity from a handle value.
        /// </summary>
        /// <returns>The entity that is identified in the drawing by the specified handle.</returns>
        public static Entity GetEntityByHandle(string handle, Transaction transaction,
            OpenMode mode, bool openErased)
        {
            // The default for opening objects in a locked layer is false.
            return GetEntityByHandle(handle, transaction, mode, openErased, false);
        }


        /// <summary>
        /// Gets an entity from a handle value.
        /// </summary>
        /// <returns>The entity that is identified in the drawing by the specified handle.</returns>
        public static Entity GetEntityByHandle(string handle, Transaction transaction,
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
        /// Prompts the user to input a double.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        /// <param name="allowNone">If false, the user is forced to enter a number or cancel.</param>
        /// <returns>The integer the user wrote. Any other operation will return 0.</returns>
        public static int GetInteger(string message, ref PromptStatus status, bool allowNone)
        {
            // Default value is 0.
            return GetInteger(message, ref status, allowNone, 0);
        }


        /// <summary>
        /// Prompts the user to input a double.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        /// <param name="allowNone">If false, the user is forced to enter a number or cancel.</param>
        /// <returns>The integer the user wrote. Any other operation will return the default value.</returns>
        public static int GetInteger(string message, ref PromptStatus status, bool allowNone,
            int defaultValue)
        {
            PromptIntegerOptions options = new PromptIntegerOptions(System.Environment.NewLine + message);
            options.AllowNone = allowNone;
            options.DefaultValue = defaultValue;
            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptIntegerResult result = command.GetInteger(options);
            status = result.Status;

            return result.Value;
        }

        /// <summary>
        /// Prompts the user to select a keyword from a list.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        /// <param name="keywords">List of keywords from which the user will choose.</param>
        /// <returns>The keyword selected by the user.</returns>
        /// <remarks>The keywords must be single words. If a keyword includes spaces, 
        /// only the first word will be returned.</remarks>
        public static string GetKeyword(string message, ref PromptStatus status, string[] keywords)
        {
            PromptKeywordOptions options = new PromptKeywordOptions(message);
            PromptResult result;
            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;
            
            foreach (string keyword in keywords)
            {
                options.Keywords.Add(keyword);
            }
            
            result = command.GetKeywords(options);
            status = result.Status;

            return result.StringResult;
        }


        /// <summary>
        /// Prompts the user to select a point in the drawing.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        public static Point3d GetPoint3d(string message, ref PromptStatus status)
        {
            PromptPointOptions options = new PromptPointOptions(message);
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
        /// <param name="message">Message to display in the command line.</param>
        public static DBObjectCollection GetSelection(string message, Transaction transaction,
            ref PromptStatus status, OpenMode mode)
        {
            // The defaults for opening erased or locked objects are both false.
            return GetSelection(message, transaction, ref status, mode, false, false);
        }


        /// <summary>
        /// Prompts the user to do a selection of one or more objects.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        public static DBObjectCollection GetSelection(string message, Transaction transaction,
            ref PromptStatus status, OpenMode mode, bool openErased)
        {
            // The default for opening objects in a locked layer is false.
            return GetSelection(message, transaction, ref status, mode, openErased, false);
        }


        /// <summary>
        /// Prompts the user to do a selection of one or more objects.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        public static DBObjectCollection GetSelection(string message, Transaction transaction,
            ref PromptStatus status, OpenMode mode, bool openErased, bool forceOpenOnLockedLayer)
        {
            PromptSelectionOptions options = new PromptSelectionOptions();
            PromptSelectionResult result;
            SelectionSet selection;
            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;
            DBObjectCollection objectsSelected = new DBObjectCollection();

            options.MessageForAdding = System.Environment.NewLine + message;
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


        /// <summary>
        /// Prompts the user to input a string.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        /// <returns>The string the user wrote. Any other operation will return an empty string.</returns>
        /// <remarks>If the user doesn't select anything, status will still return OK, not None.</remarks>
        public static string GetString(string message, ref PromptStatus status, bool allowSpaces)
        {
            return GetString(message, ref status, allowSpaces, null);
        }


        /// <summary>
        /// Prompts the user to input a string.
        /// </summary>
        /// <param name="message">Message to display in the command line.</param>
        /// <returns>The string the user wrote or the default value if nothing was written.</returns>
        /// <remarks>If the user doesn't select anything, status will still return OK, not None.</remarks>
        public static string GetString(string message, ref PromptStatus status, bool allowSpaces,
            string defaultValue)
        {
            PromptStringOptions options = new PromptStringOptions(System.Environment.NewLine + message);
            options.AllowSpaces = allowSpaces;
            options.DefaultValue = defaultValue;
            Editor command = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptResult result = command.GetString(options);
            status = result.Status;

            return result.StringResult;
        }
    }
}