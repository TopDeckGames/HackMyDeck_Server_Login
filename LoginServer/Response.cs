using System;
using System.IO;
using System.Text;
using System.Configuration;

namespace LoginServer
{
    public class Response
    {
    	private Stream binaryData;
    	private BinaryWriter writer;
    	private Encoding encoding; 
    	private static volatile string encodingName;
    	
    	public Response()
    	{
    		if(Response.encodingName == null)
    		{
				Response.encodingName = ConfigurationManager.AppSettings["binary_encoding"];
    		}
    		
    		this.binaryData = new MemoryStream();
    		this.encoding = Encoding.GetEncoding(encodingName);
    	}
    	
    	/// <summary>
    	/// Lance l'écriture dans le flux
    	/// </summary>
    	public void openWriter()
    	{
    		this.writer = new BinaryWriter(this.binaryData, this.encoding);
    	}
    	
    	/// <summary>
    	/// Stop l'écriture dans le flux
    	/// </summary>
    	public void closeWriter()
    	{
    		this.writer.Close();
    	}
    	
    	/// <summary>
    	/// Ajoute un integer dans la réponse
    	/// </summary>
    	/// <param name="value">Entier</param>
    	public void addValue(int value)
    	{
			try
    		{
    			this.writer.Write(value);
    		}
    		catch(Exception e)
    		{
    			throw new Exception("Impossible d'écrire dans le flux de réponse", e);
    		}
    	}
    	
    	/// <summary>
    	/// Ajoute un entier non signé dans la réponse
    	/// </summary>
    	/// <param name="value">Entier non signé</param>
    	public void addValue(uint value)
    	{
			try
    		{
    			this.writer.Write(value);
    		}
    		catch(Exception e)
    		{
    			throw new Exception("Impossible d'écrire dans le flux de réponse", e);
    		}
    	}
    	
    	/// <summary>
    	/// Ajoute un short dans la réponse
    	/// </summary>
    	/// <param name="value">Short</param>
    	public void addValue(short value)
    	{
			try
    		{
    			this.writer.Write(value);
    		}
    		catch(Exception e)
    		{
    			throw new Exception("Impossible d'écrire dans le flux de réponse", e);
    		}
    	}
    	
    	/// <summary>
    	/// Ajoute un short non signé dans la réponse
    	/// </summary>
    	/// <param name="value">Short non signé</param>
    	public void addValue(ushort value)
    	{
			try
    		{
    			this.writer.Write(value);
    		}
    		catch(Exception e)
    		{
    			throw new Exception("Impossible d'écrire dans le flux de réponse", e);
    		}
    	}
    	
    	/// <summary>
    	/// Ajoute un float dans la réponse
    	/// </summary>
    	/// <param name="value"></param>
    	public void addValue(float value)
    	{
			try
    		{
    			this.writer.Write(value);
    		}
    		catch(Exception e)
    		{
    			throw new Exception("Impossible d'écrire dans le flux de réponse", e);
    		}
    	}
    	
    	/// <summary>
    	/// Ajoute un tableau de char dans la réponse
    	/// </summary>
    	/// <param name="value">Tableau de char</param>
    	public void addValue(char[] value)
    	{
			try
    		{
    			this.writer.Write(value);
    		}
    		catch(Exception e)
    		{
    			throw new Exception("Impossible d'écrire dans le flux de réponse", e);
    		}
    	}
    	
    	/// <summary>
    	/// Ajoute un string dans la réponse
    	/// </summary>
    	/// <param name="value">String</param>
    	public void addValue(string value)
    	{
			try
    		{
				this.addValue(value.ToCharArray());
    		}
    		catch(Exception e)
    		{
    			throw e;
    		}
    	}
    	
    	/// <summary>
    	/// Retourne la réponse au format binaire
    	/// </summary>
    	/// <returns>Réponse</returns>
    	public byte[] getResponse()
    	{
    		byte[] result = new byte[this.binaryData.Length];
    		using(var reader = new BinaryReader(this.binaryData, this.encoding))
    		{
    			reader.Read(result, 0, int.Parse(this.binaryData.Length.ToString()));
    		}
    		return result;
    	}
    }
}