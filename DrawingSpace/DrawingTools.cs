﻿using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

public static class DrawingTools
{
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

        else if (polyline is Polyline2d)
        {
            Polyline2d pline2d = (Polyline2d)polyline;
            Database database = HostApplicationServices.WorkingDatabase;
            Transaction transaction = database.TransactionManager.StartTransaction();

            foreach (ObjectId vertexId in pline2d)
            {
                Vertex2d vertex = (Vertex2d)transaction.GetObject(vertexId, OpenMode.ForRead);
                vertices.Add(vertex.Position);
            }

        }

        else if (polyline is Polyline3d)
        {
            Polyline3d pline3d = (Polyline3d)polyline;
            Database database = HostApplicationServices.WorkingDatabase;
            Transaction transaction = database.TransactionManager.StartTransaction();

            foreach (ObjectId vertexId in pline3d)
            {
                PolylineVertex3d vertex = (PolylineVertex3d)transaction.GetObject(vertexId, OpenMode.ForRead);
                vertices.Add(vertex.Position);
            }
        }

        return vertices;
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
        DrawingSpace.Axis rotationAxis, DrawingSpace.AngleMode mode)
    {
        // Default case is rotation around de Z-axis.
        Vector3d rotateVector = new Vector3d(0, 0, 1);

        switch (rotationAxis)
        {
            case DrawingSpace.Axis.X:
                rotateVector = new Vector3d(1, 0, 0);
                break;
            case DrawingSpace.Axis.Y:
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
        Vector3d rotationAxis, DrawingSpace.AngleMode mode)
    {
        if (mode == DrawingSpace.AngleMode.Degrees)
        {
            rotationAngle = rotationAngle * 180 / Math.PI;
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
    public static DBObjectCollection SortEntities(DBObjectCollection entities, DrawingSpace.Axis axis)
    {
        DBObject[] entitiesArray = new DBObject[entities.Count - 1];
        double[] positions = new double[entities.Count - 1];

        for (int i = 0; i < entities.Count; i++)
        {
            entitiesArray[i] = entities[i];

            switch (axis)
            {
                case DrawingSpace.Axis.X:
                    positions[i] = ((Entity)entities[i]).GeometricExtents.MinPoint.X;
                    break;
                case DrawingSpace.Axis.Y:
                    positions[i] = ((Entity)entities[i]).GeometricExtents.MinPoint.Y;
                    break;
                case DrawingSpace.Axis.Z:
                    positions[i] = ((Entity)entities[i]).GeometricExtents.MinPoint.Z;
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
    public static Point3dCollection SortPoints(Point3dCollection points, DrawingSpace.Axis axis)
    {
        Point3d[] pointsArray = new Point3d[points.Count - 1];
        double[] positions = new double[points.Count - 1];

        for (int i = 0; i < points.Count; i++)
        {
            pointsArray[i] = points[i];

            switch (axis)
            {
                case DrawingSpace.Axis.X:
                    positions[i] = points[i].X;
                    break;
                case DrawingSpace.Axis.Y:
                    positions[i] = points[i].Y;
                    break;
                case DrawingSpace.Axis.Z:
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
