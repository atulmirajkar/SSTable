using System.Collections.Generic;
using System.Collections;
using System;
namespace AVLTree
{
    public class AVL<TKey, TValue>:IEnumerable<KeyValuePair<TKey, TValue>> where TKey:IComparable{

        public class Node{
            public Node left { get; set; }
            public Node right{get; set;} 
            public TKey key{get; set;}
            public TValue value{get; set;} 
            public int height{get; set;}
            public int bf { get; set; }

            public Node(TKey key, TValue value){
                this.key = key;
                this.value = value;
                height = 1;
            }
        }

        private Node root{get; set;}

        //todo implement size
        private int size{get; set;}

        private static void updateHeight(Node node){
            if(node == null)
                return;
            
            int leftHeight = node.left != null ? node.left.height : 0;
            int rightHeight = node.right != null ? node.right.height : 0;
            node.height = 1+ Math.Max(leftHeight,rightHeight);

            node.bf = leftHeight - rightHeight;   
        }

        private static Node rotateRight(Node node){
            if(node == null)
                return null;
            
            Node pivot = node.left;
            //if pivot itself is null dont do anything
            if(pivot == null)
                return node;

            Node temp = pivot.right;
            pivot.right = node;
            node.left = temp;
            
            //update heights
            //update input node's height first because it goes below the pivot node
            updateHeight(node);
            updateHeight(pivot);
            return pivot;
        }

        private static Node rotateLeft(Node node){
            if(node == null)
                return null;
            
            Node pivot = node.right;
            //if pivot itself is null dont do anything
            if(pivot == null)
                return node;
            
            Node temp = pivot.left;
            pivot.left = node;
            node.right = temp;
            

            //update heights
            updateHeight(node);
            updateHeight(pivot);

            return pivot;
        }

        private static Node rebalanceForInsert(Node node, TKey key){
            if(node == null)
                return null;

            int balance = node.bf;
            //left - left case
            if(balance > 1){
                if(key.CompareTo(node.left.key) < 0)
                    return rotateRight(node);
                
                //left - right case
                if(key.CompareTo(node.left.key) > 0){
                    node.left = rotateLeft(node.left);
                    return rotateRight(node);
                }
            }else if(balance < 1){
                //right right case
                if(key.CompareTo(node.right.key) > 0)
                    return rotateLeft(node);

                //right left case
                if(key.CompareTo(node.right.key) < 0){
                    node.right = rotateRight(node.right);
                    return rotateLeft(node);
                }
            }
            return null;
        }
       

        private Node insert(TKey key, TValue value, Node node){
            //exit condition
            if(node == null){
                return new Node(key, value);
            }

            //perform normal bst insertion
            //dont allow duplicates
            if(key.CompareTo(node.key)==0){
                return node;
            }
            if(key.CompareTo(node.key) < 0)
            {
                node.left = insert(key, value, node.left);
            }else{
                node.right = insert(key, value, node.right);
            }

            //update height as we unwind the stack
            updateHeight(node);

            //check whether bf within limits
            if(node.bf < -1 ||  node.bf > 1){
                return rebalanceForInsert(node, key);
            }
            
            return node;
        }
        private Node delete(TKey key, Node node)
        {
            //exit condition
            if (node == null)
                return null;
            
            if(key.CompareTo(node.key) < 0 )
            {
                node.left = delete(key, node.left);
            }else if(key.CompareTo(node.key) > 0)
            {
                node.right = delete(key, node.right);
            }
            else
            {
                //we have found the node
                if(node.left == null)
                {
                    return node.right;
                }else if(node.right == null)
                {
                    return node.left;
                }

                //node has 2 children - replace the node with inorder successor
                TKey minInRight = minkey(node.right);
                //copy data to the node
                node.key = minInRight;
               
                //delete the inorder successor
                node.right =  delete(minInRight, node.right);
            }
            

            //update height and balance factor
            updateHeight(node);

            if (node.bf < -1 || node.bf > 1)
                return  rebalanceForDelete(node);

            return node;

        }
        private static Node rebalanceForDelete(Node node){
            if(node == null)
                return null;

            int balance = node.bf;
            //left - left case
            if(balance > 1){
                if(node.left?.bf >= 0)
                    return rotateRight(node);
                
                //left - right case
                if(node.left?.bf<0){
                    node.left = rotateLeft(node.left);
                    return rotateRight(node);
                }
            }else if(balance < 1){
                //right right case
                if(node.right?.bf<=0)
                    return rotateLeft(node);

                //right left case
                if(node.right?.bf>0){
                    node.right = rotateRight(node.right);
                    return rotateLeft(node);
                }
            }
            return null;
        }
 
        private TKey minkey(Node node)
        {
            TKey minResult = node.key;
            while(node.left != null)
            {
                node = node.left;
                minResult = node.key;
            }
            return minResult;
        }
        private void preOrder(Node node){
            //exit condition 
            if(node == null)
                return;
            
            Console.WriteLine(node.key+":"+node.value);
            preOrder(node.left);
            preOrder(node.right);
        }

        private IEnumerable<KeyValuePair<TKey, TValue>> inorder(Node node){
            if(node == null)
                yield break;
            
            foreach(KeyValuePair<TKey, TValue> kv in inorder(node.left))
                yield return kv;
            
            yield return KeyValuePair.Create(node.key, node.value);

            foreach(KeyValuePair<TKey, TValue> kv in inorder(node.right))
                yield return kv;
        }

        //public methods
        public void PreOrder()
        {
            preOrder(root);
        }

        public Node Insert(TKey key, TValue value)
        {
            root = insert(key,value,root);
            return root;
        }

        public void Delete(TKey key)
        {
            //important to assign to root, since we could be deleting root
            root = delete(key, root);
        }

        public IEnumerator GetEnumerator()
        {
            return inorder(root).GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
           return inorder(root).GetEnumerator();
        }
    }

}