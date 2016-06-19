using System;
using System.Collections.Generic;
using System.Linq;
using Simple1C.Impl;

namespace Simple1C.Tests.TestEntities
{
    internal class TestObjectsManager
    {
        private readonly GlobalContext globalContext;
        private readonly EnumConverter enumConverter;
        private readonly string organizationInn;

        public TestObjectsManager(GlobalContext globalContext, EnumConverter enumConverter, string organizationInn)
        {
            this.globalContext = globalContext;
            this.enumConverter = enumConverter;
            this.organizationInn = organizationInn;
        }

        public dynamic ComObject
        {
            get { return globalContext.ComObject; }
        }

        public object CreateAccountingDocument(AccountingDocument document) 
        {
            var item = ComObject.���������.�����������������������.CreateDocument();
            item.����������� = GetOrganization();
            item.������������� =
                GetUserByDescription(document.IsCreatedByEmployee ? "��������.���������" : "��������.������");
            item.���������� = ((dynamic) CreateCounterparty(document.Counterpart)).������;
            item.������������������ = CreateCounterpartContract(item.����������, document.CounterpartContract).������;
            item.��������������� = GetCurrencyByCode("643");
            item.���������������� = document.SumIncludesNds;
            dynamic account;
            if (TryFindChartOfAccounts("60.01", out account))
                item.������������������������������ = account.������;
            if (TryFindChartOfAccounts("60.02", out account))
                item.�������������������������� = account.������;
            item.���������������������� = document.Date;
            item.����������������������� = document.Number;
            item.���� = document.Date;
            item.����������� = document.Comment;
            item.����������� = enumConverter.Convert(document.OperationKind);
            item.������������������� = enumConverter.Convert(AdvanceWay.Automatically);
            foreach (var nomenclatureItem in document.Items)
            {
                var nomenclatureItemAccessObject = item.������.��������();
                nomenclatureItemAccessObject.���������� = nomenclatureItem.Count;
                nomenclatureItemAccessObject.���� = nomenclatureItem.Price;
                nomenclatureItemAccessObject.��������� = enumConverter.Convert(nomenclatureItem.NdsRate);
                nomenclatureItemAccessObject.�������� = nomenclatureItem.NdsSum;
                nomenclatureItemAccessObject.����� = document.SumIncludesNds
                    ? nomenclatureItem.Sum
                    : nomenclatureItem.Sum - nomenclatureItem.NdsSum;
                nomenclatureItemAccessObject.������������ = CreateNomenclature(nomenclatureItem).������;
            }
            item.Write();
            return item;
        }

        public object CreateCounterparty(Counterpart counterpart)
        {
            var item = ComObject.�����������.�����������.CreateItem();
            item.������������������������� = enumConverter.Convert(counterpart.LegalForm);
            item.��� = counterpart.Inn;
            if (counterpart.LegalForm == LegalForm.Organization)
                item.��� = counterpart.Kpp;
            item.������������ = counterpart.Name;
            item.������������������ = counterpart.FullName ?? counterpart.Name;
            item.�������������������� = false;
            item.Write();
            return item;
        }

        private bool TryFindChartOfAccounts(string code, out dynamic result)
        {
            var findResult = ComObject.�����������.������������.FindByCode(code);
            if (findResult.IsEmpty())
            {
                result = null;
                return false;
            }
            result = findResult;
            return true;
        }

        private dynamic CreateNomenclature(NomenclatureItem nomenclatureItem)
        {
            var nomenclatureAccessObject = ComObject.�����������.������������.CreateItem();
            nomenclatureAccessObject.������������ = nomenclatureItem.Name;
            nomenclatureAccessObject.������������������ = nomenclatureItem.Name;
            nomenclatureAccessObject.������ = true;
            nomenclatureAccessObject.��������� = enumConverter.Convert(nomenclatureItem.NdsRate);
            nomenclatureAccessObject.Write();
            return nomenclatureAccessObject;
        }

        public object CreateBankAccount(object owner, BankAccount bankAccount)
        {
            var item = ComObject.�����������.���������������.CreateItem();
            item.���������� = bankAccount.Number;
            item.���� = GetBankByBic(bankAccount.Bank.Bik);
            item.������������ = bankAccount.Name ?? GenerateBankAccountName(item.����, bankAccount.Number);
            if (bankAccount.CurrencyCode != null)
                item.��������������������� = GetCurrencyByCode(bankAccount.CurrencyCode);
            item.�������� = owner;
            item.Write();
            return item;
        }

        public object CreateCounterpartContract(object counterpartReference, CounterpartyContract contract)
        {
            var item = ComObject.�����������.��������������������.CreateItem();
            item.����������� = enumConverter.Convert(contract.Kind);
            item.����������� = GetOrganization();
            item.�������� = counterpartReference;
            item.������������ = contract.Name;
            item.����������� = string.Format("test {0}", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            if (!string.IsNullOrEmpty(contract.CurrencyCode))
                item.�������������������� = GetCurrencyByCode(contract.CurrencyCode);
            item.Write();
            return item;
        }

        private static string GenerateBankAccountName(dynamic bank, string number)
        {
            return string.Format("{0}, {1}", number, bank.������������);
        }

        private object GetCurrencyByCode(string currencyCode)
        {
            return GetCatalogItemByCode("������", currencyCode);
        }

        private object GetBankByBic(string bik)
        {
            return GetCatalogItemByCode("�����", bik);
        }

        private object GetCatalogItemByKeyOrNull(string catalogName, string keyName, string keyValue)
        {
            const string queryFormat = @"
                �������
	                catalog.������ ��� ������
                ��
	                ����������.{0} ��� catalog
                ���
	                catalog.{1} = &key";
            return globalContext.Execute(string.Format(queryFormat, catalogName, keyName), new Dictionary<string, object> {{"key", keyValue}}).Select(x => x["������"]).FirstOrDefault();
        }

        private object GetUserByDescription(string name)
        {
            return GetCatalogItemByKey("������������", "������������", name);
        }

        private object GetOrganization()
        {
            return GetCatalogItemByKey("�����������", "���", organizationInn);
        }

        private object GetCatalogItemByCode(string catalogName, string code)
        {
            return GetCatalogItemByKey(catalogName, "���", code);
        }

        private object GetCatalogItemByKey(string catalogName, string keyName, string keyValue)
        {
            var result = GetCatalogItemByKeyOrNull(catalogName, keyName, keyValue);
            if (result == null)
            {
                const string messageFormat = "catalog [{0}] item with {1} [{2}] not found";
                throw new InvalidOperationException(string.Format(messageFormat, catalogName, keyName, keyValue));
            }
            return result;
        }
    }
}