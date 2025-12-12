using ProManSystem.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ProManSystem.Views
{
    public partial class CustomerCardWindow : Window
    {
        private readonly Customer _customer;

        public CustomerCardWindow(Customer customer)
        {
            InitializeComponent();
            _customer = customer;
            LoadCustomerData();
        }

        private void LoadCustomerData()
        {
           
            SousTitreTextBlock.Text = $"{_customer.CodeClient} - {_customer.NomComplet}";

            CodeTextBlock.Text = _customer.CodeClient;
            NomTextBlock.Text = _customer.NomComplet;
            ActiviteTextBlock.Text = _customer.Activite;
            AdresseTextBlock.Text = _customer.Adresse;
            RcTextBlock.Text = _customer.NumeroRC;
            MatriculeTextBlock.Text = _customer.MatriculeFiscal;
            TypeIdTextBlock.Text = _customer.TypeIdentification;
            NumeroIdTextBlock.Text = _customer.NumeroIdentification;
            CategorieTextBlock.Text = _customer.Categorie;
            DateCreationTextBlock.Text = _customer.DateCreation.ToString("dd/MM/yyyy");

            StatutTextBlock.Text = _customer.EstRadie ? "Radié" : "Actif";
            StatutTextBlock.Foreground = _customer.EstRadie
                ? new SolidColorBrush(Colors.Red)
                : new SolidColorBrush(Colors.Green);

            RemarquesTextBlock.Text = _customer.EstRadie
                ? "Client radié. Voir la date de radiation dans la fiche détaillée."
                : "Client actif.";
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new PrintDialog();
                if (dlg.ShowDialog() == true)
                {
                   
                    dlg.PrintVisual(CardRootGrid, $"Carte client {_customer.CodeClient}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'impression : " + ex.Message,
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
          
            MessageBox.Show("Fonction d'enregistrement (PDF / image) à implémenter plus tard.",
                "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
