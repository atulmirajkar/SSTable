﻿using System;

namespace AVLTree
{
    class Treebehavior
    {
        static void Main(string[] args)
        {
            AVL tree = new AVL();
            tree.Insert(10);
            tree.Insert(5);
            tree.Insert(7);
            tree.PreOrder();

            Console.WriteLine();
            tree.Delete(7);
            tree.PreOrder();
        }
    }
}