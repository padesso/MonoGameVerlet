using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGameVerlet.Verlet;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonoGameVerlet.DataStructures
{
	//https://github.com/SoDamnMetal/Spatial-Partition/blob/main/QuadTrees/VerletComponents/QuadTree.cs

	public class QuadTree
	{
		private readonly int MAX_OBJECTS = 1;
		private readonly int MAX_LEVELS = 150;

		private int level;
		private List<VerletComponent> objects; //convert to your object type
		private List<VerletComponent> returnObjects;
		private RectangleF bounds;
		private QuadTree[] nodes;


		public QuadTree(int pLevel, RectangleF pBounds)
		{
			level = pLevel;
			objects = new List<VerletComponent>();
			bounds = pBounds;
			nodes = new QuadTree[4];
			returnObjects = new List<VerletComponent>();
		}

		public void Clear()
		{
			objects.Clear();

			for (int i = 0; i < nodes.Length; i++)
			{
				if (nodes[i] != null)
				{
					nodes[i].Clear();
					nodes[i] = null;
				}
			}
		}


		private void Split()
		{
			float subWidth = bounds.Width / 2;
			float subHeight = bounds.Height / 2;
			float x = bounds.X;
			float y = bounds.Y;

			nodes[0] = new QuadTree(level + 1, new RectangleF(x + subWidth, y, subWidth, subHeight));
			nodes[1] = new QuadTree(level + 1, new RectangleF(x, y, subWidth, subHeight));
			nodes[2] = new QuadTree(level + 1, new RectangleF(x, y + subHeight, subWidth, subHeight));
			nodes[3] = new QuadTree(level + 1, new RectangleF(x + subWidth, y + subHeight, subWidth, subHeight));
		}


		// Determine which node the object belongs to. -1 means
		// object cannot completely fit within a child node and is part
		// of the parent node

		private int GetIndex(VerletComponent pRect)
		{
			int index = -1;
			double verticalMidpoint = bounds.X + (bounds.Width / 2f);
			double horizontalMidpoint = bounds.Y + (bounds.Height / 2f);

			// Object can completely fit within the top quadrants
			bool topQuadrant = (pRect.Bounds.Y < horizontalMidpoint && pRect.Bounds.Y + pRect.Bounds.Height < horizontalMidpoint);
			// Object can completely fit within the bottom quadrants
			bool bottomQuadrant = (pRect.Bounds.Y > horizontalMidpoint);

			// Object can completely fit within the left quadrants
			if (pRect.Bounds.X < verticalMidpoint && bounds.X + pRect.Bounds.Width < verticalMidpoint)
			{
				if (topQuadrant)
				{
					index = 1;
				}
				else if (bottomQuadrant)
				{
					index = 2;
				}
			}
			// Object can completely fit within the right quadrants
			else if (pRect.Bounds.X >= verticalMidpoint)
			{
				if (topQuadrant)
				{
					index = 0;
				}
				else if (bottomQuadrant)
				{
					index = 3;
				}
			}

			return index;
		}

		public void Insert(VerletComponent pRect)
		{
			if (nodes[0] != null)
			{
				int index = GetIndex(pRect);

				if (index != -1)
				{
					nodes[index].Insert(pRect);

					return;
				}
			}

			objects.Add(pRect);

			if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS)
			{
				if (nodes[0] == null)
				{
					Split();
				}

				int i = 0;
				while (i < objects.Count)
				{
					int index = GetIndex(objects[i]);
					if (index != -1)
					{
						nodes[index].Insert(objects[i]);
						objects.RemoveAt(i);
					}
					else
					{
						i++;
					}
				}
			}
		}


		// Return all objects that could collide with the given object (recursive)

		public void Retrieve(List<VerletComponent> returnedObjs, VerletComponent obj)
		{
			if (nodes[0] != null)
			{
				var index = GetIndex(obj);
				if (index != -1)
				{
					nodes[index].Retrieve(returnedObjs, obj);
				}
				else
				{
					for (int i = 0; i < nodes.Length; i++)
					{
						nodes[i].Retrieve(returnedObjs, obj);
					}
				}
			}
			returnedObjs.AddRange(objects);
		}

		public void Draw(SpriteBatch spriteBatch, GraphicsDevice g)
		{
			foreach (QuadTree node in nodes)
			{
				if (node != null)
				{
					node.Draw(spriteBatch, g);
					ShapeExtensions.DrawRectangle(spriteBatch, new RectangleF(node.bounds.Left, node.bounds.Top, node.bounds.Width, node.bounds.Height), Color.White);
					ShapeExtensions.DrawRectangle(spriteBatch, new RectangleF(node.bounds.Left, node.bounds.Top, node.bounds.Width, node.bounds.Height), Color.White);
				}
			}
		}

		public void Update(GameTime gameTime, List<VerletComponent> VerletComponents)
		{

			Clear();
			foreach (var m in VerletComponents)
			{
				Insert(m);
			}

			//for (int i = 0; i < VerletComponents.Count; i++)
			//{
			//	returnObjects.Clear();
			//	Retrieve(returnObjects, VerletComponents[i]);

			//	for (int x = 0; x < returnObjects.Count; x++)
			//	{
			//		// Run collision detection algorithm between objects
			//		// i.e. your Rectangle.IntersectsWith(x)

			//		// Don't check for collisions with self object
			//		if (VerletComponents[i] == returnObjects[x]) { continue; }

			//		if (VerletComponents[i].Bounds.Intersects(returnObjects[x].Bounds))
			//		{
			//			VerletComponents[i].Velocity = -VerletComponents[i].Velocity;
			//		}
			//	}
			//	VerletComponents[i].Update(gameTime);
			//}
		}
	}
}
