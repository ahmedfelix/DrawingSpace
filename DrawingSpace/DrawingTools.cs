namespace CADUtilities
{
    using System;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;

    public static class DrawingTools
    {
        /// <summary>
        /// Creates a Polyline with default settings (layer, color, etc.) with the specified vertices. 
        /// Only the X and Y coordinates are taken into account, width is zero, and no bulges are added.
        /// </summary>
        /// <param name="vertices">Vertices that will compose the polyline.</param>
        public static Polyline CreatePolyline(Point3dCollection vertices)
        {
            Polyline polyline = new Polyline();
            polyline.SetDatabaseDefaults();

            for (int i = 0; i < vertices.Count; i++)
            {
                polyline.AddVertexAt(i, new Point2d(vertices[i].X, vertices[i].Y), 0, 0, 0);
            }

            return polyline;
        }

        /// <summary>
        /// Gets the midpoint between two points.
        /// </summary>
        public static Point3d GetMidpoint(Point3d point1, Point3d point2)
        {
            return new Point3d((point1.X + point2.X) / 2,
                (point1.Y + point2.Y) / 2, (point1.Z + point2.Z) / 2);
        }

        /// <summary>
        /// Gets the vertices of a polyline.
        /// </summary>
        /// <param name="polyline">Polyline, Polyline2d, or Polyline3d from which to obtain the vertices</param>
        public static Point3dCollection GetVertices(Curve polyline)
        {
            Point3dCollection vertices = new Point3dCollection();

            if (polyline is Polyline)
            {
                Polyline pline = (Polyline)polyline;

                for (int i = 0; i < pline.NumberOfVertices; i++)
                {
                    vertices.Add(pline.GetPoint3dAt(i));
                }
            }
            else
            {
                Database database = HostApplicationServices.WorkingDatabase;
                Transaction transaction = database.TransactionManager.StartTransaction();

                if (polyline is Polyline2d)
                {
                    Polyline2d pline2d = (Polyline2d)polyline;

                    foreach (Object vertex in pline2d)
                    {
                        if (vertex is Vertex2d)
                        {
                            vertices.Add(((Vertex2d)vertex).Position);
                        }
                        else if (vertex is ObjectId)
                        {
                            vertices.Add(((Vertex2d)transaction.GetObject((ObjectId)vertex, OpenMode.ForRead)).Position);
                        }
                    }
                }

                else if (polyline is Polyline3d)
                {
                    Polyline3d pline3d = (Polyline3d)polyline;

                    foreach (ObjectId vertexId in pline3d)
                    {
                        PolylineVertex3d vertex = (PolylineVertex3d)transaction.GetObject(vertexId, OpenMode.ForRead);
                        vertices.Add(vertex.Position);
                    }
                }

                transaction.Dispose();
            }

            return vertices;
        }

        /// <summary>
        /// Inserts a block in the drawing.
        /// </summary>
        /// <param name="name">Name of the block to insert.</param>
        /// <param name="insertionPoint">Point where the block will be inserted.</param>
        /// <returns></returns>
        public static BlockReference InsertBlockReference(string name, Point3d insertionPoint, Transaction transaction)
        {
            Database database = HostApplicationServices.WorkingDatabase;
            ObjectId blockId;

            BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[name], OpenMode.ForRead);

            blockId = blockTableRecord.Id;

            BlockReference block = new BlockReference(insertionPoint, blockId);
            DrawingSpace.AddEntity(block, transaction);

            return block;
        }

        /// <summary>
        /// Inserts a block in the drawing.
        /// </summary>
        /// <param name="name">Name of the block to insert.</param>
        /// <param name="insertionPoint">Point where the block will be inserted.</param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static BlockReference InsertBlockReference(string name, Point3d insertionPoint,
            string[] attributes, Transaction transaction)
        {
            BlockReference block = InsertBlockReference(name, insertionPoint, transaction);
            Database database = HostApplicationServices.WorkingDatabase;

            BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[name], OpenMode.ForRead);

            AttributeReference attribute;
            AttributeDefinition attributeDefinition;
            int attributeIndex = 0;
            Entity entity;

            foreach (ObjectId id in blockTableRecord)
            {
                entity = (Entity)transaction.GetObject(id, OpenMode.ForWrite, false, true);

                if (entity.GetType() == typeof(AttributeDefinition))
                {
                    attributeDefinition = (AttributeDefinition)entity;
                    attribute = new AttributeReference();
                    attribute.SetPropertiesFrom(attributeDefinition);
                    attribute.Position = new Point3d(attributeDefinition.Position.X + block.Position.X,
                        attributeDefinition.Position.Y + block.Position.Y,
                        attribute.Position.Z + block.Position.Z);

                    attribute.Height = attributeDefinition.Height;
                    attribute.Rotation = attributeDefinition.Rotation;
                    attribute.Tag = attributeDefinition.Tag;
                    attribute.TextString = attributes[attributeIndex];
                    attributeIndex++;

                    block.AttributeCollection.AppendAttribute(attribute);
                    transaction.AddNewlyCreatedDBObject(attribute, true);
                }
            }

            return block;
        }

        /// <summary>
        /// Moves an object in the drawing.
        /// </summary>
        public static void Move(Entity entity, Point3d fromPoint, Point3d toPoint)
        {
            Vector3d moveVector = new Vector3d(toPoint.X - fromPoint.X, toPoint.Y - fromPoint.Y,
                toPoint.Z - fromPoint.Z);
            Matrix3d moveMatrix = Matrix3d.Displacement(moveVector);

            entity.TransformBy(moveMatrix);
        }

        /// <summary>
        /// Rotates an object in the drawing.
        /// </summary>
        /// <param name="rotationAngle">Angle in decimal degrees.</param>
        /// <param name="rotationAxis">X, Y, or Z axis around wich the rotation will take place.</param>
        /// <remarks>This method performs a 2D rotation around the specified axis.</remarks>
        public static void Rotate(Entity entity, Point3d basePoint, double rotationAngle,
            Axis rotationAxis, AngleMode mode)
        {
            // Default case is rotation around de Z-axis.
            Vector3d rotateVector = new Vector3d(0, 0, 1);

            switch (rotationAxis)
            {
                case Axis.X:
                    rotateVector = new Vector3d(1, 0, 0);
                    break;
                case Axis.Y:
                    rotateVector = new Vector3d(0, 1, 0);
                    break;
            }

            Rotate(entity, basePoint, rotationAngle, rotateVector, mode);
        }

        /// <summary>
        /// Rotates an object in the drawing.
        /// </summary>
        /// <param name="rotationAxis">Vector representing the custom axis around 
        /// which the rotation will take place.</param>
        /// <param name="mode">States if the angle entered is in degrees or radians.</param>
        public static void Rotate(Entity entity, Point3d basePoint, double rotationAngle,
            Vector3d rotationAxis, AngleMode mode)
        {
            if (mode == AngleMode.Degrees)
            {
                rotationAngle = rotationAngle * Math.PI / 180;
            }

            Matrix3d rotateMatrix = Matrix3d.Rotation(rotationAngle, rotationAxis, basePoint);

            entity.TransformBy(rotateMatrix);
        }

        /// <summary>
        /// Scales an object in the drawing.
        /// </summary>
        public static void Scale(Entity entity, Point3d basePoint, double scale)
        {
            Matrix3d scaleMatrix = Matrix3d.Scaling(scale, basePoint);
            entity.TransformBy(scaleMatrix);
        }
        /// <summary>
        /// Sorts a DBObjectCollection so that the order of its items corresponds 
        /// to the position of these items along the axis parameter. 
        /// </summary>
        /// <param name="items">Entities that will be sorted.</param>
        /// <param name="axis">Axis along which the entities will be sorted.</param>
        /// <remarks>The entity's position is obtained from the GeometricExtents.MinPoint property.</remarks>
        public static DBObjectCollection SortEntities(DBObjectCollection entities, Axis axis)
        {
            return SortEntities(entities, axis, SortParameter.MinPoint);
        }

        /// <summary>
        /// Sorts a DBObjectCollection so that the order of its items corresponds 
        /// to the position of these items along the axis parameter. 
        /// </summary>
        /// <param name="entities">Entities that will be sorted.</param>
        /// <param name="axis">Axis along which the entities will be sorted.</param>
        /// <param name="sortParameter">Defines what point in the object's GeometricExtents will be used to sort.</param>
        /// <remarks>The entity's position is obtained from the GeometricExtents and uses what the sortParameter indicates.</remarks>
        public static DBObjectCollection SortEntities(DBObjectCollection entities, Axis axis, SortParameter sortParameter)
        {
            DBObject[] entitiesArray = new DBObject[entities.Count];
            double[] positions = new double[entities.Count];

            for (int i = 0; i < entities.Count; i++)
            {
                Entity entity = (Entity)entities[i];
                entitiesArray[i] = entity;

                switch (axis)
                {
                    case Axis.X:
                        switch (sortParameter)
                        {
                            case SortParameter.MinPoint:
                                positions[i] = entity.GeometricExtents.MinPoint.X;
                                break;
                            case SortParameter.CenterPoint:
                                positions[i] = (entity.GeometricExtents.MinPoint.X + entity.GeometricExtents.MaxPoint.X) / 2;
                                break;
                            case SortParameter.MaxPoint:
                                positions[i] = entity.GeometricExtents.MinPoint.X;
                                break;
                            default:
                                break;
                        }
                        break;
                    case Axis.Y:
                        switch (sortParameter)
                        {
                            case SortParameter.MinPoint:
                                positions[i] = entity.GeometricExtents.MinPoint.Y;
                                break;
                            case SortParameter.CenterPoint:
                                positions[i] = (entity.GeometricExtents.MinPoint.Y + entity.GeometricExtents.MaxPoint.Y) / 2;
                                break;
                            case SortParameter.MaxPoint:
                                positions[i] = entity.GeometricExtents.MinPoint.Y;
                                break;
                            default:
                                break;
                        }
                        break;
                    case Axis.Z:
                        switch (sortParameter)
                        {
                            case SortParameter.MinPoint:
                                positions[i] = entity.GeometricExtents.MinPoint.Z;
                                break;
                            case SortParameter.CenterPoint:
                                positions[i] = (entity.GeometricExtents.MinPoint.Z + entity.GeometricExtents.MaxPoint.Z) / 2;
                                break;
                            case SortParameter.MaxPoint:
                                positions[i] = entity.GeometricExtents.MinPoint.Z;
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }

            Array.Sort(positions, entitiesArray);

            DBObjectCollection sortedEntities = new DBObjectCollection();
            for (int i = 0; i < entitiesArray.Length; i++)
            {
                sortedEntities.Add(entitiesArray[i]);
            }

            return sortedEntities;
        }

        /// <summary>
        /// Sorts a Point3dCollection so that the order of its points corresponds 
        /// to their position along the axis parameter. 
        /// </summary>
        /// <param name="points">Points that will be sorted.</param>
        /// <param name="axis">Axis along which the points will be sorted.</param>
        public static Point3dCollection SortPoints(Point3dCollection points, Axis axis)
        {
            Point3d[] pointsArray = new Point3d[points.Count];
            double[] positions = new double[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                pointsArray[i] = points[i];

                switch (axis)
                {
                    case Axis.X:
                        positions[i] = points[i].X;
                        break;
                    case Axis.Y:
                        positions[i] = points[i].Y;
                        break;
                    case Axis.Z:
                        positions[i] = points[i].Z;
                        break;
                }
            }

            Array.Sort(positions, pointsArray);

            Point3dCollection sortedPoints = new Point3dCollection();
            for (int i = 0; i < pointsArray.Length; i++)
            {
                sortedPoints.Add(pointsArray[i]);
            }

            return sortedPoints;
        }
    }
}