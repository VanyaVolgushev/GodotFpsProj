using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public static class CustomMath
{
	public static Vector2 ProjectionCoord(Vector3 vector, Vector3 planeNormal)
    {
		if(planeNormal == Vector3.Up)
		{
			return new Vector2(vector.X, vector.Z);
		}
		else
		{
			Vector3 proj = vector - vector.Dot(planeNormal) * planeNormal;
			Vector3 upProj = (Vector3.Up - Vector3.Up.Dot(planeNormal) * planeNormal).Normalized();
			Vector3 rightProj = planeNormal.Cross(upProj).Normalized();
			return new Vector2(rightProj.Dot(proj), upProj.Dot(proj));
		}
    }
    public static List<Vector3> SortClockwise(List<Vector3> vectors, Vector3 planeNormal)
    {
        return vectors.OrderBy(v => Mathf.Atan2(ProjectionCoord(v, planeNormal).Y,
											    ProjectionCoord(v, planeNormal).X)).ToList();
    }

	public static Vector3 SlideAlongMultiple(Vector3 translation, Vector3 initialTranslation, List<Vector3> collisionNormals)
	{
		if(collisionNormals.Count == 0)
		{
			return translation;
		}
		else if(collisionNormals.Count == 1)
		{
			if(collisionNormals[0].Dot(translation) <= 0)
			{
				return MoveAndSlideAlongPlane(translation, collisionNormals[0], 0).projectedLeftOver;
			}
			else
			{
				return translation;
			}
		}
        else if (collisionNormals.Count == 2)
        {
            Vector3 creaseVector = collisionNormals[0].Cross(collisionNormals[1]);
            if (creaseVector != Vector3.Zero)
            {
                creaseVector = creaseVector.Normalized();
            }
			Vector3 innerCreasePlane0 = collisionNormals[0].Cross(creaseVector).Normalized();
			Vector3 innerCreasePlane1 = creaseVector.Cross(collisionNormals[1]).Normalized();
			bool rightSideOfCreasePlane0 = innerCreasePlane0.Dot(initialTranslation) > 0;
			bool rightSideOfCreasePlane1 = innerCreasePlane1.Dot(initialTranslation) > 0;
			if(rightSideOfCreasePlane0 && rightSideOfCreasePlane1)
			{
				//moving into crease
            	return SlideAlongCrease(translation, creaseVector);
			}
			else if(!rightSideOfCreasePlane0 && !rightSideOfCreasePlane1)
			{
				//moving outside crease
				return translation;
			}
			else if(!rightSideOfCreasePlane0)
			{
				//moving along plane0
				return MoveAndSlideAlongPlane(initialTranslation.Normalized() * translation.Length(), collisionNormals[0], 0).projectedLeftOver;
			}
			else if(!rightSideOfCreasePlane1)
			{
				//moving along plane1
				return MoveAndSlideAlongPlane(initialTranslation.Normalized() * translation.Length(), collisionNormals[1], 0).projectedLeftOver;
			}
			else
			{
				
				GD.Print("ERROR: moving neither outside nor inside crease");
				return Vector3.Zero;
			}
		}
		else if(collisionNormals.Count == 3)
		{
			Vector3 cornerDir = collisionNormals.Aggregate((a, b) => a + b).Normalized();
			collisionNormals = SortClockwise(collisionNormals, cornerDir);
			List<Vector3> creasePlanes = new();

			for(int i = 0; i < collisionNormals.Count - 1; i++)
			{
				creasePlanes.Add(-collisionNormals[i + 1].Cross(collisionNormals[i]).Normalized());
			}
			creasePlanes.Add(-collisionNormals[0].Cross(collisionNormals[collisionNormals.Count - 1]).Normalized());
			List<(Vector3 vector, int index)> wrongSideCreasePlanes = new();
			for(int i = 0; i < creasePlanes.Count; i++)
			{
				if(creasePlanes[i].Dot(initialTranslation) < 0)
				{
					wrongSideCreasePlanes.Add((creasePlanes[i], i));
				}
			}
			if(wrongSideCreasePlanes.Count == 0)
			{
				//moving inside corner
				return Vector3.Zero;
			}
			if(wrongSideCreasePlanes.Count == 1)
			{
				//moving along a crease
            	return SlideAlongCrease(initialTranslation.Normalized() * translation.Length(), wrongSideCreasePlanes[0].vector);
			}
			else if(wrongSideCreasePlanes.Count == 2)
			{
				//moving along a plane
				Vector3 slidePlane = wrongSideCreasePlanes[1].vector.Cross(wrongSideCreasePlanes[0].vector).Normalized();
				return MoveAndSlideAlongPlane(initialTranslation.Normalized() * translation.Length(), slidePlane, 0f).projectedLeftOver;
			}
			else if(wrongSideCreasePlanes.Count == 3)
			{
				//moving outside
				GD.Print("wrong side of 3 crease planes");
				return translation;
			}
			else
			{
				throw new Exception("ERROR: wrong side of 4 crease planes");
			}
		}
		else return Vector3.Zero;
	}
	public static (Vector3 toSurface, Vector3 projectedLeftOver) MoveAndSlideAlongPlane(Vector3 translation, Vector3 collisionNormal, float collisionSafeFraction)
	{
		Vector3 leftOverTranslation = translation * (1 - collisionSafeFraction);
		Vector3 translationToSurface = translation * collisionSafeFraction;
		Vector3 projectedLeftOverTranslation = leftOverTranslation - leftOverTranslation.Dot(collisionNormal) * collisionNormal;
		return (translationToSurface, projectedLeftOverTranslation);
	}
	public static Vector3 SlideAlongCrease(Vector3 translation, Vector3 creaseVector)
	{
		float projectionMagnitude = translation.Dot(creaseVector);
        Vector3 projectedLeftOver = creaseVector * projectionMagnitude;
        return projectedLeftOver;
	}
	public static Vector3 Invert(Vector3 vec)
	{
		return new Vector3(1f/vec.X, 1f/vec.Y, 1f/vec.Z);
	}
}